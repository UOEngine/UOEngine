using System.Diagnostics;
using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;
using VkBuffer = Silk.NET.Vulkan.Buffer;

namespace UOEngine.Runtime.Rendering
{
    public class RenderStagingBuffer: IDisposable
    {
        public VkBuffer                 Handle => _buffer;

        private readonly RenderDevice   _device;

        private VkBuffer                _buffer;
        private DeviceMemory            _deviceMemory;

        private ulong                   _sizeInBytes;

        public RenderStagingBuffer(RenderDevice device, ulong sizeInBytes)
        {
            _device = device;

            _sizeInBytes = sizeInBytes;

            // Coherent for now. More manual down the line.
            _device.CreateBuffer(sizeInBytes, BufferUsageFlags.TransferSrcBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, out _buffer, out _deviceMemory);
        }

        public unsafe void Map<T>(ReadOnlySpan<T> dataToStage)
        {
            Debug.Assert(((ulong)dataToStage.Length * (ulong)Unsafe.SizeOf<T>()) == _sizeInBytes);

            void* data;

            Vulkan.VkMapMemory(_device.Handle, _deviceMemory, 0, _sizeInBytes, 0, &data);

            dataToStage.CopyTo(new Span<T>(data, (int)_sizeInBytes));

            Vulkan.VkUnmapMemory(_device.Handle, _deviceMemory);
        }

        public void Dispose()
        {
            Vulkan.vkDestroyBuffer(_device.Handle, _buffer);
            _buffer.Handle = 0;

            Vulkan.VkFreeMemory(_device.Handle, _deviceMemory);
            _deviceMemory.Handle = 0;
        }
    }
}
