// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Vortice.Vulkan;

using UOEngine.Runtime.Core;

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

    private readonly VulkanDevice _device;

    private readonly uint _mappedBufferSize;

    private List<AllocationInfo> _freeAllocations = [];

    private VulkanMemoryAllocation _allocation;

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
        _device.MemoryManager.AllocateBuffer(_mappedBufferSize, VkBufferUsageFlags.TransferSrc, VkMemoryPropertyFlags.HostVisible, out _allocation, "StagingBuffer");
    }

    internal VulkanStagingBufferLock AcquireBuffer(uint size)
    {
        uint bytesLeft = _allocation.Size - _used;

        if(bytesLeft < size)
        {
            UOEDebug.Assert(false);
        }

        if (_used + size > _allocation.Size)
        {
            UOEDebug.Assert(false);
        }

        nint offset = _allocation.GetMappedPointer(_device) + (nint)_used;

        var bufferLock = new VulkanStagingBufferLock
        {
            buffer = new Span<byte>((void*)offset, (int)size),
            Offset = _used,
            vkBuffer = _allocation.Buffer
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
