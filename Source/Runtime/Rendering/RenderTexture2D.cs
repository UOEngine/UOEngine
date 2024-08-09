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
        R8G8B8A8
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
        public RenderTexture2D(RenderTexture2DDescription description, RenderDevice renderDevice)
        {
            _description = description;
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
                result = vk.CreateImage(renderDevice.Device, ref imageCreateInfo, null, out _image);
            }

            Debug.Assert(result == Result.Success);

            vk.GetImageMemoryRequirements(renderDevice.Device, _image, out var memoryRequirements);

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

                result = vk.BindImageMemory(renderDevice.Device, _image, deviceTextureMemory, 0);
            }

            Debug.Assert(result == Result.Success);

            _imageView = renderDevice.CreateImageView(_image, textureFormat);

        }

        ~RenderTexture2D()
        {

        }

        public void Upload<T>(ReadOnlySpan<T> texels)
        {
            ulong bufferSize = _imageSize;

            _renderDevice.CreateBuffer
            (
                bufferSize,
                BufferUsageFlags.TransferSrcBit,
                MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
                out var stagingBuffer,
                out var stagingBufferMemory
            );

            var vk = Vk.GetApi();

            unsafe
            {
                void* data;

                Vk.GetApi().MapMemory(_renderDevice.Device, stagingBufferMemory, 0, bufferSize, 0, &data);
                texels.CopyTo(new Span<T>(data, (int)bufferSize));
            }

            Vk.GetApi().UnmapMemory(_renderDevice.Device, stagingBufferMemory);

            using (var commandList = _renderDevice.BeginRecording())
            {
                // buffer to image

                TransitionImageLayout(commandList, ImageLayout.Undefined, ImageLayout.TransferDstOptimal);

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
                        Width = _description.Width,
                        Height = _description.Height,
                        Depth = 1
                    }
                };

                commandList.CopyBufferToImage(stagingBuffer, _image, ImageLayout.TransferDstOptimal, 1, bufferImageCopy);

                TransitionImageLayout(commandList, ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal);
            }

            unsafe
            {
                vk.DestroyBuffer(_renderDevice.Device, stagingBuffer, null);
                vk.FreeMemory(_renderDevice.Device, stagingBufferMemory, null);
            }
        }

        public void Upload(IntPtr texels)
        {
            Upload(new ReadOnlySpan<byte>());
        }

        public void SubresourceTransition(ERenderSubresourceState newState)
        {
            using (var commandBuffer = _renderDevice.BeginRecording())
            {
                if ((_state == ERenderSubresourceState.Undefined) && (newState == ERenderSubresourceState.ShaderResource))
                {
                    TransitionImageLayout(commandBuffer, ImageLayout.Undefined, ImageLayout.TransferDstOptimal);
                }
            }
        }

        private void TransitionImageLayout(RenderCommandList commandBuffer, ImageLayout oldLayout, ImageLayout newLayout)
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
                Image = _image,
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

            commandBuffer.PipelineBarrier(sourceStage, destinationStage, imageMemoryBarrier);
        }

        public ImageView                                   _imageView { get; private set; }
        private Image                                      _image;

        private readonly RenderTexture2DDescription      _description;
        private RenderDevice                             _renderDevice;
        private readonly uint                            _imageSize;
        private ERenderSubresourceState                  _state = ERenderSubresourceState.Undefined;         
    }
}
