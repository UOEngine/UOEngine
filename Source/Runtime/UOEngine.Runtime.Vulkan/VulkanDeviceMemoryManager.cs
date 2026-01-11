// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Vortice.Vulkan;

using UOEngine.Runtime.Core;

namespace UOEngine.Runtime.Vulkan;

internal class VulkanDeviceMemoryAllocation
{
    internal VkDeviceMemory Handle { get; private set; }
    internal IntPtr MappedPtr;
    internal ulong Size;
    internal ulong Offset;

    private readonly VulkanDevice _device;

    internal VulkanDeviceMemoryAllocation(VulkanDevice device, ulong size, uint offset, VkDeviceMemory gpuMemory)
    {
        _device = device;
        Size = size;
        Handle = gpuMemory;
        Offset = offset;
    }

    internal bool IsMapped => MappedPtr != IntPtr.Zero;

    internal unsafe Span<byte> Map()
    {
        if (MappedPtr == IntPtr.Zero)
        {
            void* ptr;

            _device.Api.vkMapMemory(_device.Handle, Handle, Offset, Size, VkMemoryMapFlags.None, &ptr);

            MappedPtr = (IntPtr)ptr;
        }

        return new Span<byte>((void*)MappedPtr, (int)Size);
    }

    internal void Unmap()
    {
        _device.Api.vkUnmapMemory(_device.Handle, Handle);

        Handle = VkDeviceMemory.Null;
    }

    internal void FlushMappedMemory(uint offset, uint size)
    {
        UOEDebug.Assert(IsMapped);
        UOEDebug.Assert(offset + size <= Size);

        VkMappedMemoryRange mappedMemoryRange = new()
        {
            memory = Handle,
            offset = offset,
            size = size
        };

        var result = _device.Api.vkFlushMappedMemoryRanges(_device.Handle, mappedMemoryRange);
    }
}

internal class VulkanDeviceMemoryManager
{
    private readonly VulkanDevice _device;

    private ulong _totalAllocated;

    private List<VulkanDeviceMemoryAllocation> _allocations = [];

    internal VulkanDeviceMemoryManager(VulkanDevice device)
    {
        _device = device;
    }

    internal unsafe VulkanDeviceMemoryAllocation Allocate(ulong size, uint memoryTypeIndex, VkMemoryPropertyFlags memoryPropertyFlags)
    {
        VkMemoryAllocateInfo memoryAllocateInfo = new()
        {
            allocationSize = size,
            memoryTypeIndex = memoryTypeIndex,
        };


        VkResult result = _device.Api.vkAllocateMemory(_device.Handle, &memoryAllocateInfo, out var gpuMemory);

        if(result == VkResult.ErrorOutOfDeviceMemory || result == VkResult.ErrorOutOfHostMemory)
        {
            throw new OutOfMemoryException("Out of device memory!");
        }

        VulkanDeviceMemoryAllocation deviceAllocation = new(_device, size, 0, gpuMemory);

        _totalAllocated += size;

        _allocations.Add(deviceAllocation);

        return deviceAllocation;
    }
}
