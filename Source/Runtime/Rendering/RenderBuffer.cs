using Silk.NET.Vulkan;
using System.Diagnostics;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace UOEngine.Runtime.Rendering
{
    [Flags]
    public enum ERenderBufferUsageFlags
    {
        None = 0,
    }

    [Flags]
    public enum EMemoryPropertyFlags
    {
        None = 0
    }

    public class RenderBuffer
    {
        public unsafe RenderBuffer(uint size, ERenderBufferUsageFlags usageFlags, EMemoryPropertyFlags propertyFlags, RenderDevice renderDevice) 
        {
            BufferUsageFlags bufferusageflags = BufferUsageFlags.None;

            BufferCreateInfo bufferCreateInfo = new()
            {
                SType = StructureType.BufferCreateInfo,
                Size = size,
                Usage = bufferusageflags,
                SharingMode = SharingMode.Exclusive
            };

            var vk = Vk.GetApi();

            var result = vk.CreateBuffer(renderDevice.Device, ref bufferCreateInfo, null, out _hostBuffer);

            Debug.Assert(result == Result.Success);

            vk.GetBufferMemoryRequirements(renderDevice.Device, _hostBuffer, out var memoryRequirements);

            MemoryPropertyFlags memoryPropertyFlags = MemoryPropertyFlags.None;

            MemoryAllocateInfo memoryAllocateInfo = new()
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = memoryRequirements.Size,
                MemoryTypeIndex = renderDevice.FindMemoryType(memoryRequirements.MemoryTypeBits, memoryPropertyFlags)
            };

            result = vk.AllocateMemory(renderDevice.Device, ref memoryAllocateInfo, null, out _deviceMemory);

            Debug.Assert(result == Result.Success);

            result = vk.BindBufferMemory(renderDevice.Device, _hostBuffer, _deviceMemory, 0);

            Debug.Assert(result == Result.Success);
        }

        private readonly Buffer         _hostBuffer;
        private readonly DeviceMemory   _deviceMemory;
    }
}
