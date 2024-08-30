using Silk.NET.SDL;
using Silk.NET.Vulkan;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using UOEngine.Runtime.Rendering.Resources;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace UOEngine.Runtime.Rendering
{
    public enum ERenderBufferType
    {
        None = 0,
        Vertex,
        Index
    }

    [Flags]
    public enum EMemoryPropertyFlags
    {
        None = 0
    }

    public class RenderBuffer: RenderResource
    {
        public bool IsMarkedForDelete { get; private set; } = false;

        public unsafe RenderBuffer(ERenderBufferType bufferType, RenderDevice renderDevice) 
        {
            _renderDevice = renderDevice;
            _device = renderDevice.Device;

            _stagingBuffer = default;
            _stagingBufferMemory = default;

            _deviceBuffer = default;
            _deviceBufferMemory = default;

            Length = 0;

            _deviceBufferUsage = BufferUsageFlags.None;

            switch(bufferType)
            {
                case ERenderBufferType.Vertex:
                    {
                        _deviceBufferUsage = BufferUsageFlags.VertexBufferBit;
                    }
                    break;

                case ERenderBufferType.Index:
                    {
                        _deviceBufferUsage = BufferUsageFlags.IndexBufferBit;
                    }
                    break;

                default:
                    throw new NotSupportedException();  
            }
        }

        public void MarkForDelete()
        {
            _renderDevice.ImmediateContext!.MarkResourceForDelete(this);
            IsMarkedForDelete = true;
        }

        public override void Destroy()
        {
            Debug.Assert(IsMarkedForDelete);

            Vulkan.VkDestroyBuffer(_renderDevice.Handle, _deviceBuffer);

            _deviceBuffer.Handle = 0;

            Vulkan.VkFreeMemory(_renderDevice.Handle, _stagingBufferMemory);
            _stagingBufferMemory.Handle = 0;

            Vulkan.VkDestroyBuffer(_renderDevice.Handle, _stagingBuffer);
            _stagingBuffer.Handle = 0;

            Vulkan.VkFreeMemory(_renderDevice.Handle, _deviceBufferMemory);
            _deviceBufferMemory.Handle = 0;
        }

        public unsafe void CopyToDevice<T>(ReadOnlySpan<T> uploadData)
        {
            ulong sizeOfType = (ulong)Unsafe.SizeOf<T>(); 

            var bufferSize = sizeOfType * (ulong)uploadData.Length;

            Length = (uint)uploadData.Length;

            //Debug.Assert(_stagingBuffer.Handle == 0);
            //Debug.Assert(_stagingBufferMemory.Handle == 0);

            CreateBuffer(bufferSize, BufferUsageFlags.TransferSrcBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, out _stagingBuffer, out _stagingBufferMemory);

            CreateBuffer(bufferSize, BufferUsageFlags.TransferDstBit | _deviceBufferUsage, MemoryPropertyFlags.DeviceLocalBit, out _deviceBuffer, out _deviceBufferMemory);

            void* data;

            Vk.GetApi().MapMemory(_device, _stagingBufferMemory, 0, bufferSize, 0, &data);
            uploadData.CopyTo(new Span<T>(data, (int)bufferSize));
            Vk.GetApi().UnmapMemory(_device, _stagingBufferMemory);

            BufferCopy copyRegion = new()
            {
                Size = bufferSize,
            };

            Vk.GetApi().CmdCopyBuffer(_renderDevice.ImmediateContext!.CommandBufferManager.GetUploadCommandBuffer()!.Handle, _stagingBuffer, _deviceBuffer, 1, ref copyRegion);
        }

        public void CopyToDevice<T>(IntPtr ptr, int size)
        {

        }
        private void CopyToDeviceInternal()
        {

        }
   
        private unsafe void CreateBuffer(ulong size, BufferUsageFlags usage, MemoryPropertyFlags properties, out Buffer buffer, out DeviceMemory bufferMemory)
        {
            BufferCreateInfo bufferCreateInfo = new()
            {
                SType = StructureType.BufferCreateInfo,
                Size = size,
                Usage = usage,
                SharingMode = SharingMode.Exclusive
            };

            var vk = Vk.GetApi();

            var result = vk.CreateBuffer(_device, ref bufferCreateInfo, null, out buffer);

            Debug.Assert(result == Result.Success);

            vk.GetBufferMemoryRequirements(_device, buffer, out var memoryRequirements);

            MemoryAllocateInfo memoryAllocateInfo = new()
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = memoryRequirements.Size,
                MemoryTypeIndex = _renderDevice.FindMemoryType(memoryRequirements.MemoryTypeBits, properties)
            };

            result = vk.AllocateMemory(_device, ref memoryAllocateInfo, null, out bufferMemory);

            Debug.Assert(result == Result.Success);

            result = vk.BindBufferMemory(_device, buffer, bufferMemory, 0);

            Debug.Assert(result == Result.Success);
        }

        public Buffer                       DeviceBuffer => _deviceBuffer;
        public uint                         Length { get; private set; }

        private Buffer                      _stagingBuffer;
        private DeviceMemory                _stagingBufferMemory;

        private Buffer                      _deviceBuffer;
        private DeviceMemory                _deviceBufferMemory;

        private readonly Device             _device;
        private readonly RenderDevice       _renderDevice;
        private readonly BufferUsageFlags   _deviceBufferUsage;
        //private readonly IEnumerable<T>     _data;
    }
}
