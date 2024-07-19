using System.Diagnostics;

using Silk.NET.Vulkan;

namespace UOEngine.Runtime.Rendering
{
    public enum ERenderSubresourceState
    {
        None = 0,
        Read,
        RenderTarget,
        ShaderResource,
    }

    public enum ERenderTextureFormat
    {
        None,
        R5G5B5A1
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
        public unsafe RenderTexture2D(RenderTexture2DDescription description, byte[] texels, RenderDevice renderDevice)
        {
            _description = description;
            _texels = texels;

            Format textureFormat = Format.Undefined;

            switch(description.Format)
            {
                case ERenderTextureFormat.R5G5B5A1 :
                    {
                        textureFormat = Format.R5G5B5A1UnormPack16;
                    }
                    break;

                default:
                    Debug.Assert(false);
                    break;
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

            var result = vk.CreateImage(renderDevice.Device, ref imageCreateInfo, null, out _image);

            Debug.Assert(result == Result.Success);

            vk.GetImageMemoryRequirements(renderDevice.Device, _image, out var memoryRequirements);

            MemoryAllocateInfo memoryAllocateInfo = new()
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = memoryRequirements.Size,
                MemoryTypeIndex = renderDevice.FindMemoryType(memoryRequirements.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit)
            };

            result = vk.AllocateMemory(renderDevice.Device, ref memoryAllocateInfo, null, out var deviceTextureMemory);

            Debug.Assert(result == Result.Success);

            result = vk.BindImageMemory(renderDevice.Device, _image, deviceTextureMemory, 0);

            Debug.Assert(result == Result.Success);

        }

        public void SubresourceTransition(ERenderSubresourceState newState)
        {

        }

        //private ImageView?                      _imageView;
        private Image                           _image;

        private RenderTexture2DDescription      _description;
        private byte[]                          _texels;
    }
}
