// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Runtime.InteropServices;
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

internal class VulkanScratchBlockAllocator
{
    private struct Block
    {
        internal VulkanMemoryAllocation MemoryAllocation;
        internal IntPtr MappedPtr;
        internal uint Offset;
    }

    private List<Block> _blocks = [];

    private Block _currentBlock;

    private readonly VulkanDevice _device;

    private readonly uint _sizePerBlock = 4 * 1024 * 1024;

    internal VulkanScratchBlockAllocator(VulkanDevice device)
    {
        _device = device;

        _currentBlock = AllocateBlock();
    }

    internal Span<byte> Allocate(uint size, out VulkanMemoryAllocation memoryAllocation)
    {
         var blockToUse = AllocateInternal(size, out var allocationOffset);

        memoryAllocation = new VulkanMemoryAllocation
        {
            Buffer = blockToUse.MemoryAllocation.Buffer,
            Offset = allocationOffset,
            Size = size,
            DeviceMemoryAllocation = blockToUse.MemoryAllocation.DeviceMemoryAllocation
        };

        return memoryAllocation.Map();
    }

    internal void Reset()
    {
        var blocks = CollectionsMarshal.AsSpan(_blocks);

        _currentBlock = blocks[0];

        for(int i = 0; i < blocks.Length; i++)
        {
            blocks[i].Offset = 0;
        }
    }

    private Block AllocateBlock()
    {
        var block = new Block();

        _device.MemoryManager.AllocateBuffer(_sizePerBlock, VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferSrc, VkMemoryPropertyFlags.HostVisible, out block.MemoryAllocation);

        block.MappedPtr = block.MemoryAllocation.MappedPtr;

        _blocks.Add(block);

        return block;

    }

    private Block AllocateInternal(uint size, out uint allocationoffset)
    {
        if(size + _currentBlock.Offset >  _sizePerBlock)
        {
           _currentBlock = AllocateBlock();
        }

        allocationoffset = _currentBlock.Offset;

        _currentBlock.Offset += size;

        return _currentBlock;
        
    }
}
