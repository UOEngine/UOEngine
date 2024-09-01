using System.Diagnostics;
using System.Runtime.InteropServices;

using Silk.NET.Vulkan;
using VkBuffer = Silk.NET.Vulkan.Buffer;

namespace UOEngine.Runtime.Rendering.Resources
{
    public class RenderUniformBuffer: RenderResource
    {
        public VkBuffer        Handle => _handle;

        public ulong            Size { get; private set; }

        private RenderDevice    _device;

        private DeviceMemory    _deviceMemory;

        private VkBuffer        _handle;

        public unsafe RenderUniformBuffer(RenderDevice device, ulong size)
        {
            _device = device;
            Size = size;

            device.CreateBuffer(Size, BufferUsageFlags.UniformBufferBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, out _handle, out _deviceMemory);
        }

        public override void Destroy()
        {
            Vulkan.vkDestroyBuffer(_device.Handle, _handle);

            _handle.Handle = 0;

            Vulkan.VkFreeMemory(_device.Handle, _deviceMemory);

            _deviceMemory.Handle = 0;
        }

        public unsafe void Update<T>(T value)
        {
            Debug.Assert(Marshal.SizeOf(value) == (int)Size);

            void* data;
            Vulkan.VkMapMemory(_device.Handle, _deviceMemory, 0, Size, MemoryMapFlags.None, &data);

            new Span<T>(data, 1)[0] = value;

            Vulkan.VkUnmapMemory(_device.Handle, _deviceMemory);

        }
    }
}
