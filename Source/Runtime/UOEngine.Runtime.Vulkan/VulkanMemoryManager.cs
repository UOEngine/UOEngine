// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

internal struct VulkanMemoryAllocation
{
    internal VkBuffer Buffer;
    internal uint Offset;
    internal uint Size;
    internal VulkanDeviceMemoryAllocation DeviceMemoryAllocation;

    internal Span<byte> Map() => DeviceMemoryAllocation.Map();
    internal IntPtr MappedPtr => DeviceMemoryAllocation.MappedPtr;

    internal void FlushMappedMemory(uint offset, uint size) => DeviceMemoryAllocation.FlushMappedMemory(offset, size);
}

internal class VulkanMemoryManager
{
    private VulkanDevice _device;

    private ulong _totalAllocated;

    internal VulkanMemoryManager(VulkanDevice device)
    {
        _device = device;
    }

    internal void Init()
    {

    }

    internal bool AllocateUniformBuffer(uint size, out VulkanMemoryAllocation memoryAllocation)
    {
        return AllocateBuffer(size, VkBufferUsageFlags.UniformBuffer, VkMemoryPropertyFlags.HostCoherent | VkMemoryPropertyFlags.HostVisible, out memoryAllocation);
    }

    internal unsafe bool AllocateBuffer(uint size, VkBufferUsageFlags usage, VkMemoryPropertyFlags memoryProperties, out VulkanMemoryAllocation memoryAllocation)
    {
        VkBufferCreateInfo bufferCreateInfo = new()
        {
            size = size,
            usage = usage
        };

        memoryAllocation = new();

        _device.Api.vkCreateBuffer(_device.Handle, &bufferCreateInfo, out memoryAllocation.Buffer);

        _device.Api.vkGetBufferMemoryRequirements(_device.Handle, memoryAllocation.Buffer, out var memoryRequirements);

        memoryAllocation.DeviceMemoryAllocation = _device.DeviceMemoryManager.Allocate(memoryRequirements.size, _device.GetMemoryTypeIndex(memoryRequirements.memoryTypeBits, memoryProperties), memoryProperties);

        _device.Api.vkBindBufferMemory(_device.Handle, memoryAllocation.Buffer, memoryAllocation.DeviceMemoryAllocation.Handle, 0);

        _totalAllocated += size;

        if(memoryProperties.HasFlag(VkMemoryPropertyFlags.HostVisible))
        {
            memoryAllocation.DeviceMemoryAllocation.Map();
        }

        return true;
    }

    internal void AllocateBufferMemory()
    {

    }
}
