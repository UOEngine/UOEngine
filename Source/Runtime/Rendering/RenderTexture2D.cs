using System.Diagnostics;

using Silk.NET.Vulkan;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UOEngine.Runtime.Rendering
{
    public enum ERenderSubresourceState
    {
        Undefined = 0,
        Read,
        RenderTarget,
        ShaderResource,
    }

    public enum ERenderTextureFormat
    {
        None,
        A1R5G5B5,
        R5G5B5A1,
        R8G8B8A8,
        B8G8R8A8
    }

    [Flags]
    public enum ERenderTextureUsage
    {
        None,
        Staging,
    }
    public struct RenderTexture2DDescription
    {
        public uint                    Width;
        public uint                    Height;
        public ERenderTextureFormat    Format;
    }

    public class RenderTexture2D
    {
        public RenderTexture2DDescription   Description { get; private set; }
        public ImageView?                   ShaderResourceView { get; private set; }

        private Image?                      _image;

        private RenderDevice                _renderDevice;
        private readonly uint               _imageSize;

        private RenderStagingBuffer?        _stagingBuffer; 

        public RenderTexture2D(RenderDevice renderDevice, RenderTexture2DDescription description)
        {
            Description = description;
            _renderDevice = renderDevice;

            Format textureFormat = Format.Undefined;

            switch(description.Format)
            {
                case ERenderTextureFormat.A1R5G5B5 :
                    {
                        textureFormat = Format.A1R5G5B5UnormPack16;
                        _imageSize = sizeof(ushort) * description.Width * description.Height;
                    }
                    break;

                case ERenderTextureFormat.R8G8B8A8:
                    {
                        textureFormat = Format.R8G8B8A8Unorm;
                        _imageSize = sizeof(uint) * description.Width * description.Height;
                    }
                    break;

                default:
                    throw new Exception($"RenderTexture2D: Format {description.Format} not added in constructor.");
            }

            ImageCreateInfo imageCreateInfo = new()
            {
                SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.Type2D,
                Extent = new(description.Width, description.Height, 1),
                MipLevels = 1,
                ArrayLayers = 1,
                Format = textureFormat,
                Tiling = ImageTiling.Optimal,
                InitialLayout = ImageLayout.Undefined,
                Usage = ImageUsageFlags.TransferDstBit | ImageUsageFlags.SampledBit,
                SharingMode = SharingMode.Exclusive,
                Samples = SampleCountFlags.Count1Bit
            };

            var vk = Vk.GetApi();

            Result result = Result.Success;

            unsafe
            {
                result = vk.CreateImage(renderDevice.Device, ref imageCreateInfo, null, out Image image);

                _image = image;
            }

            Debug.Assert(result == Result.Success);

            vk.GetImageMemoryRequirements(renderDevice.Device, _image!.Value, out var memoryRequirements);

            MemoryAllocateInfo memoryAllocateInfo = new()
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = memoryRequirements.Size,
                MemoryTypeIndex = renderDevice.FindMemoryType(memoryRequirements.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit)
            };

            unsafe
            {
                result = vk.AllocateMemory(renderDevice.Device, ref memoryAllocateInfo, null, out var deviceTextureMemory);

                Debug.Assert(result == Result.Success);

                result = vk.BindImageMemory(renderDevice.Device, _image.Value, deviceTextureMemory, 0);
            }

            Debug.Assert(result == Result.Success);

            ShaderResourceView = renderDevice.CreateImageView(_image.Value, textureFormat);

            _stagingBuffer = new RenderStagingBuffer(renderDevice, _imageSize);

        }

        public RenderTexture2D(RenderDevice device, RenderTexture2DDescription description, Image image)  
        {
            Description = description;
            _renderDevice = device;
            _image = image;

            ShaderResourceView = _renderDevice.CreateImageView(_image.Value, RenderCommon.TextureFormatToVulkanFormat(description.Format));
        }

        public void DestroyShaderResourceView()
        {
            if (ShaderResourceView != null)
            {
                Vulkan.VkDestroyImageView(_renderDevice.Handle, ShaderResourceView.Value);
                ShaderResourceView = null;
            }
        }
        public void Destroy()
        {
            DestroyShaderResourceView();

            if(_image != null)
            {
                Vulkan.VkDestroyImage(_renderDevice.Handle, _image.Value);

                _image = null;
            }

            _stagingBuffer?.Dispose();
        }

        public void Upload<T>(ReadOnlySpan<T> texels)
        {
            //ulong bufferSize = _imageSize;

            //_renderDevice.CreateBuffer
            //(
            //    bufferSize,
            //    BufferUsageFlags.TransferSrcBit,
            //    MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
            //    out var stagingBuffer,
            //    out var stagingBufferMemory
            //);

            //var vk = Vk.GetApi();

            _stagingBuffer!.Map(texels);

            //unsafe
            //{
            //    void* data;

            //    Vk.GetApi().MapMemory(_renderDevice.Device, stagingBufferMemory, 0, bufferSize, 0, &data);
            //    texels.CopyTo(new Span<T>(data, (int)bufferSize));
            //}

            //Vk.GetApi().UnmapMemory(_renderDevice.Device, stagingBufferMemory);

            {
                // buffer to image

                TransitionImageLayout(_renderDevice.ImmediateContext!, ImageLayout.Undefined, ImageLayout.TransferDstOptimal);

                BufferImageCopy bufferImageCopy = new()
                {
                    BufferOffset = 0,
                    BufferRowLength = 0,
                    BufferImageHeight = 0,
                    ImageSubresource = new()
                    {
                        AspectMask = ImageAspectFlags.ColorBit,
                        MipLevel = 0,
                        BaseArrayLayer = 0,
                        LayerCount = 1
                    },
                    ImageOffset = new(0, 0),
                    ImageExtent = new()
                    {
                        Width = Description.Width,
                        Height = Description.Height,
                        Depth = 1
                    }
                };

                Vulkan.VkCmdCopyBufferToImage(_renderDevice.ImmediateContext!.CommandBufferManager.GetUploadCommandBuffer()!.Handle, 
                    _stagingBuffer.Handle, 
                    _image!.Value, 
                    ImageLayout.TransferDstOptimal, 
                    1, 
                    ref bufferImageCopy);

                TransitionImageLayout(_renderDevice.ImmediateContext!, ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal);
            }

            _renderDevice.ImmediateContext!.CommandBufferManager.SubmitUploadBuffer();
            _renderDevice.WaitUntilIdle();
        }

        public unsafe void Upload(IntPtr texels)
        {
            throw new NotImplementedException();

            //uint size = Description.Width * Description.Height * 5;

            //Upload(new ReadOnlySpan<byte>(texels.ToPointer(), (int)size));
        }

        //public void SubresourceTransition(ERenderSubresourceState newState)
        //{
        //    if ((_state == ERenderSubresourceState.Undefined) && (newState == ERenderSubresourceState.ShaderResource))
        //    {
        //        TransitionImageLayout(_renderDevice.ImmediateContext!, ImageLayout.Undefined, ImageLayout.TransferDstOptimal);
        //    }
        //}

        private unsafe void TransitionImageLayout(RenderCommandListContext context, ImageLayout oldLayout, ImageLayout newLayout)
        {
            var vk = Vk.GetApi();

            PipelineStageFlags sourceStage = PipelineStageFlags.None;
            PipelineStageFlags destinationStage = PipelineStageFlags.None;

            AccessFlags srcAccessMask = AccessFlags.None;
            AccessFlags dstAccessMask = AccessFlags.None;

            if (oldLayout == ImageLayout.Undefined && newLayout == ImageLayout.TransferDstOptimal)
            {
                dstAccessMask = AccessFlags.TransferWriteBit;

                sourceStage = PipelineStageFlags.TopOfPipeBit;
                destinationStage = PipelineStageFlags.TransferBit;
            }
            else if (oldLayout == ImageLayout.TransferDstOptimal && (newLayout == ImageLayout.ShaderReadOnlyOptimal))
            {
                srcAccessMask = AccessFlags.TransferWriteBit;
                dstAccessMask = AccessFlags.ShaderReadBit;

                sourceStage = PipelineStageFlags.TransferBit;
                destinationStage = PipelineStageFlags.FragmentShaderBit;
            }
            else
            {
                throw new Exception("Unsupported layout transition");
            }

            ImageMemoryBarrier imageMemoryBarrier = new()
            {
                SType = StructureType.ImageMemoryBarrier,
                OldLayout = oldLayout,
                NewLayout = newLayout,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
                DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Image = _image!.Value,
                SubresourceRange = new()
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                },
                SrcAccessMask = srcAccessMask,
                DstAccessMask = dstAccessMask,
            };

            vk.CmdPipelineBarrier(context.CommandBufferManager.GetUploadCommandBuffer()!.Handle, sourceStage, destinationStage, 0, 0, null, 0, null, 1, ref imageMemoryBarrier);
        }
    }
}
