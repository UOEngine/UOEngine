// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Vortice.Vulkan;

using UOEngine.Runtime.Core;
using System.Diagnostics;

namespace UOEngine.Runtime.Vulkan;

[DebuggerDisplay("VulkanSubresourceAllocator {_name}")]
internal class VulkanSubresourceAllocator
{
    internal readonly VkBufferUsageFlags Usage;
    internal readonly VkMemoryPropertyFlags MemoryProperties;
    internal readonly int AllocatorIndex;

    internal nint MappedPointer => GetMappedPointer();

    [DebuggerDisplay("FreeRange {Offset}, {Size}")]

    private struct FreeRange
    {
        internal uint Offset;
        internal uint Size;
    }

    [DebuggerDisplay("AllocatedBlock {Offset}, {Size}")]
    private struct AllocatedBlock
    {
        internal uint Offset;
        internal uint Size;
    }

    private readonly VulkanDeviceMemoryAllocation _deviceMemoryAllocation;

    private List<FreeRange> _freeList = [];
    private List<AllocatedBlock> _allocatedBlocks = [];
    private VkBuffer _buffer;
    private uint _usedSize = 0;

    private readonly string? _name;

    internal VulkanSubresourceAllocator(uint size, VkBufferUsageFlags usage, VkMemoryPropertyFlags memoryProperties, VkBuffer buffer, 
                                        VulkanDeviceMemoryAllocation deviceMemoryAllocation, int allocatorIndex, string? context = null)
    {
        Usage = usage;
        MemoryProperties = memoryProperties;
        _buffer = buffer;

        AllocatorIndex = allocatorIndex;

        _deviceMemoryAllocation = deviceMemoryAllocation;

        _name = context;

        _freeList.Add(new FreeRange
        {
            Offset = 0,
            Size = size
        });
    }

    internal bool TryAllocate(uint size, out VulkanMemoryAllocation memoryAllocation)
    {
        memoryAllocation = new();

        for (int i = 0; i < _freeList.Count; i++)
        {
            if (size > _freeList[i].Size)
            {
                continue;
            }

            _usedSize += size;
            
            memoryAllocation.Size = size;
            memoryAllocation.Offset = _freeList[i].Offset;
            memoryAllocation.AllocatorIndex = AllocatorIndex;
            memoryAllocation.Buffer = _buffer;

            var allocatedBlock = new AllocatedBlock
            {
                Size = size,
                Offset = memoryAllocation.Offset
            };

            memoryAllocation.AllocatedBlockIndex = _allocatedBlocks.Count;

            _allocatedBlocks.Add(allocatedBlock);

            _freeList.Clear();

            return true;
        }


        return false;
    }

    internal void Free(in VulkanMemoryAllocation allocation)
    {
        _freeList.Add(new FreeRange
        {
            Offset = 0,
            Size = allocation.Size
        });

        _allocatedBlocks.Clear();
    }

    internal void Flush(uint offset, uint size)
    {
        _deviceMemoryAllocation.FlushMappedMemory(offset, size);
    }

    internal nint GetMappedPointer()
    {
        UOEDebug.Assert(_deviceMemoryAllocation.IsMapped);

        return _deviceMemoryAllocation.MappedPtr;
    }
}
