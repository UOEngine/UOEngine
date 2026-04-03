// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Core;
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

internal struct VulkanMemoryAllocation
{
    internal VkBuffer Buffer;
    internal uint Offset;
    internal uint Size;
    internal int AllocatorIndex = -1;
    internal int AllocatedBlockIndex;

    internal bool IsValid => Size != 0;

    public VulkanMemoryAllocation()
    {

    }

    internal unsafe Span<byte> Map(VulkanDevice device)
    {
        var allocator = GetSubresourceAllocator(device);

        return new Span<byte>((void*)(allocator.MappedPointer + Offset), (int)Size);
    }

    internal void FlushMappedMemory(VulkanDevice device) => GetSubresourceAllocator(device).Flush(Offset, Size);

    internal VulkanSubresourceAllocator GetSubresourceAllocator(VulkanDevice device)
    {
        UOEDebug.Assert(AllocatorIndex != -1);

        return device.MemoryManager.GetSubresourceAllocator(AllocatorIndex);
    }

    VkDeviceMemory GetDeviceMemoryHandle(VulkanDevice device) => GetSubresourceAllocator(device).MemoryAllocation.Handle;

    internal void BindImage(VulkanDevice device, VkImage image)
    {
        var result = device.Api.vkBindImageMemory(device.Handle, image, GetDeviceMemoryHandle(device), Offset);

        UOEDebug.Assert(result != VkResult.ErrorOutOfDeviceMemory);
        UOEDebug.Assert(result != VkResult.ErrorOutOfHostMemory);
    }

    internal nint GetMappedPointer(VulkanDevice device)
    {
        return GetSubresourceAllocator(device).GetMappedPointer();
    }
}

internal class VulkanMemoryManager
{
    private VulkanDevice _device;

    private ulong _totalAllocated;

    private ulong _maxAllocated = 1024 * 1024 * 512;

    private List<VulkanSubresourceAllocator> _freeBufferAllocations = [];

    private List<VulkanSubresourceAllocator> _subresourceAllocations = [];

    private List<VulkanSubresourceAllocator> _textureAllocations = [];

    private Lock _imageMemoryLock = new();
    private Lock _bufferMemoryLock = new();

    internal VulkanMemoryManager(VulkanDevice device)
    {
        _device = device;
    }

    internal void Init()
    {

    }

    internal bool AllocateImageMemory(VkMemoryPropertyFlags memoryProperties, VkMemoryRequirements memoryRequirements, out VulkanMemoryAllocation memoryAllocation)
    {
        lock(_imageMemoryLock)
        {
            uint memoryTypeIndex = _device.GetMemoryTypeIndex(memoryRequirements.memoryTypeBits, memoryProperties);

            var deviceAllocation = _device.DeviceMemoryManager.Allocate(memoryRequirements.size, memoryTypeIndex, memoryProperties);

            var newTextureAllocation = new VulkanSubresourceAllocator((uint)memoryRequirements.size, memoryProperties, deviceAllocation, _subresourceAllocations.Count);

            _textureAllocations.Add(newTextureAllocation);
            _subresourceAllocations.Add(newTextureAllocation);

            return newTextureAllocation.TryAllocate((uint)memoryRequirements.size, out memoryAllocation);
        }
    }

    internal void FreeImage(in VulkanMemoryAllocation allocation)
    {
        UOEDebug.NotImplemented();
    }

    internal bool AllocateUniformBuffer(uint size, out VulkanMemoryAllocation memoryAllocation)
    {
        return AllocateBuffer(size, VkBufferUsageFlags.UniformBuffer, VkMemoryPropertyFlags.HostCoherent | VkMemoryPropertyFlags.HostVisible, out memoryAllocation);
    }

    internal unsafe bool AllocateBuffer(uint size, VkBufferUsageFlags usage, VkMemoryPropertyFlags memoryProperties, out VulkanMemoryAllocation memoryAllocation, string? context = null)
    {
        lock(_bufferMemoryLock)
        {
            for (int i = 0; i < _freeBufferAllocations.Count; i++)
            {
                if ((_freeBufferAllocations[i].Usage == usage) && (_freeBufferAllocations[i].MemoryProperties == memoryProperties))
                {
                    if (_freeBufferAllocations[i].TryAllocate(size, out memoryAllocation))
                    {
                        _freeBufferAllocations.RemoveAt(i);

                        return true;
                    }
                }
            }

            uint bufferSize = size;// Math.Max(size, 1024 * 1024);

            VkBufferCreateInfo bufferCreateInfo = new()
            {
                size = bufferSize,
                usage = usage
            };

            _device.Api.vkCreateBuffer(_device.Handle, &bufferCreateInfo, out var buffer);

            _device.Api.vkGetBufferMemoryRequirements(_device.Handle, buffer, out var memoryRequirements);

            var deviceAllocation = _device.DeviceMemoryManager.Allocate(memoryRequirements.size, _device.GetMemoryTypeIndex(memoryRequirements.memoryTypeBits, memoryProperties), memoryProperties);

            _device.Api.vkBindBufferMemory(_device.Handle, buffer, deviceAllocation.Handle, 0);

            _totalAllocated += bufferSize;

            if (memoryProperties.HasFlag(VkMemoryPropertyFlags.HostVisible))
            {
                deviceAllocation.Map();
            }

            var subresourceAllocation = new VulkanSubresourceAllocator(bufferSize, usage, memoryProperties, buffer, deviceAllocation, _subresourceAllocations.Count, context);

            _subresourceAllocations.Add(subresourceAllocation);

            if (subresourceAllocation.TryAllocate(size, out memoryAllocation))
            {
                return true;
            }

            return false;
        }
    }

    internal void Free(in VulkanMemoryAllocation memoryAllocation, bool defer)
    {
        if (defer)
        {
            _device.DeferredDeletionQueue.EnqueueResourceAllocation(memoryAllocation);

            return;
        }

        _subresourceAllocations[memoryAllocation.AllocatorIndex].Free(memoryAllocation);

        _freeBufferAllocations.Add(_subresourceAllocations[memoryAllocation.AllocatorIndex]);

    }

    internal VulkanSubresourceAllocator GetSubresourceAllocator(int allocatorIndex) => _subresourceAllocations[allocatorIndex];
}
