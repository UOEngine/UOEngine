// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Vortice.Vulkan;

using UOEngine.Runtime.Core;
using System.Buffers;

namespace UOEngine.Runtime.Vulkan;

internal struct VulkanStagingBufferLock
{
    internal VulkanMappedMemoryManager buffer;
    internal uint Offset;
    internal VkBuffer vkBuffer;
}

internal unsafe class VulkanMappedMemoryManager : MemoryManager<byte>
{
    public readonly uint Length;

    private readonly byte* _pointer;

    internal VulkanMappedMemoryManager(void* pointer, uint length)
    {
        _pointer = (byte*)pointer;
        Length = length;
    }

    public override Span<byte> GetSpan() => new(_pointer, (int)Length);

    public override MemoryHandle Pin(int elementIndex = 0) => new(_pointer + elementIndex);

    public override void Unpin(){}

    protected override void Dispose(bool disposing){}
}

internal unsafe class VulkanStagingBuffer : IDisposable
{
    private uint _used = 0;

    private readonly VulkanDevice _device;

    private readonly uint _mappedBufferSize;

    private List<AllocationInfo> _freeAllocations = [];
    private List<AllocationInfo> _usedAllocations = [];

    private VulkanMemoryAllocation _allocation;

    private Lock _acquireLock = new();

    private struct AllocationInfo
    {
        internal uint Offset;
        internal uint Size;
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
        lock (_acquireLock)
        {
            uint bytesLeft = _allocation.Size - _used;

            if (bytesLeft < size)
            {
                UOEDebug.NotImplemented();
            }

            if (_used + size > _allocation.Size)
            {
                UOEDebug.NotImplemented();
            }

            nint offset = _allocation.GetMappedPointer(_device) + (nint)_used;

            var bufferLock = new VulkanStagingBufferLock
            {
                buffer = new VulkanMappedMemoryManager((void*)offset, size),
                Offset = _used,
                vkBuffer = _allocation.Buffer
            };

            _usedAllocations.Add(new AllocationInfo
            {
                Offset = _used,
                Size = size
            });

            _used += size;

            return bufferLock;
        }
    }

    internal void ReleaseBuffer(in VulkanStagingBufferLock bufferLock)
    {
        // ToDo: temp as currently do immediate uploads so the buffer never becomes full.
        _used = 0;
    }

    internal void HackBufferFree()
    {
        lock (_acquireLock) 
        { 
            _usedAllocations.Clear();
            _used = 0; 
        }// Quick as we wait for now..
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
