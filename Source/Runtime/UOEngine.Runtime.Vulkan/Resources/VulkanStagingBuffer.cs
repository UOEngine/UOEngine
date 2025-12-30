// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Core;
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

internal ref struct VulkanStagingBufferLock
{
    internal Span<byte> buffer;
    internal uint Offset;
    internal VkBuffer vkBuffer;
}

internal unsafe class VulkanStagingBuffer: IDisposable
{
    private uint _used = 0;

    private VkBuffer _buffer;
    private readonly VulkanDevice _device;

    private byte* _mappedbufferPtr;
    private readonly uint _mappedBufferSize;

    private List<AllocationInfo> _freeAllocations = [];

    private struct AllocationInfo
    {

    }

    internal VulkanStagingBuffer(VulkanDevice device)
    {
        _device = device;
        _mappedBufferSize = 1024 * 1024 * 8; // 8MB
    }

    internal unsafe void Init()
    {
        VkBufferCreateInfo transferBuffer = new()
        {
            size = _mappedBufferSize,
            usage = VkBufferUsageFlags.TransferSrc
        };

        _device.Api.vkCreateBuffer(_device.Handle, &transferBuffer, out _buffer);

        _device.Api.vkGetBufferMemoryRequirements(_device.Handle, _buffer, out VkMemoryRequirements memReqs);

        VkMemoryAllocateInfo memAlloc = new()
        {
            allocationSize = memReqs.size,
            // Request a host visible memory type that can be used to copy our data do
            // Also request it to be coherent, so that writes are visible to the GPU right after unmapping the buffer
            memoryTypeIndex = _device.GetMemoryTypeIndex(memReqs.memoryTypeBits, VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent)
        };

        _device.Api.vkAllocateMemory(_device.Handle, &memAlloc, null, out VkDeviceMemory stagingBufferMemory);

        void* mappedMemory;

        _device.Api.vkMapMemory(_device.Handle, stagingBufferMemory, 0, memAlloc.allocationSize, 0, &mappedMemory);

        _mappedbufferPtr = (byte*)mappedMemory;

        _device.Api.vkBindBufferMemory(_device.Handle, _buffer, stagingBufferMemory, 0);

        // Now in system RAM but visible to GPU to copy. 
    }

    internal VulkanStagingBufferLock AcquireBuffer(uint size)
    {
        uint bytesLeft = _mappedBufferSize - _used;

        if(bytesLeft < size)
        {
            UOEDebug.Assert(false);
        }

        if (_used + size > _mappedBufferSize)
        {
            UOEDebug.Assert(false);
        }

        byte* offset = _mappedbufferPtr + _used;

        var bufferLock = new VulkanStagingBufferLock
        {
            buffer = new Span<byte>(offset, (int)size),
            Offset = _used,
            vkBuffer = _buffer
        };

        _used += size;

        return bufferLock;
    }

    internal void ReleaseBuffer(in VulkanStagingBufferLock bufferLock)
    {
       
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
