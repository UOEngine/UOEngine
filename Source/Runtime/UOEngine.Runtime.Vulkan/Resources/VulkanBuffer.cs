// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.RHI;
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

internal class VulkanBuffer: IDisposable
{
    public readonly VkBuffer Handle;

    private readonly VulkanDevice _device;
    private readonly VkDeviceMemory _gpuBufferMemory;

    internal unsafe VulkanBuffer(VulkanDevice device, uint size, VkBufferUsageFlags usage)
    {
        _device = device;

        VkBufferCreateInfo bufferCreateInfo = new()
        {
            size = size,
            usage = usage
        };

        device.Api.vkCreateBuffer(device.Handle, &bufferCreateInfo, out Handle);

        device.Api.vkGetBufferMemoryRequirements(device.Handle, Handle, out var memoryRequirements);

        VkMemoryAllocateInfo memoryAllocateInfo = new()
        {
            allocationSize = memoryRequirements.size,
            memoryTypeIndex = device.GetMemoryTypeIndex(memoryRequirements.memoryTypeBits, VkMemoryPropertyFlags.DeviceLocal),
        };

        device.Api.vkAllocateMemory(device.Handle, &memoryAllocateInfo, out _gpuBufferMemory);

        device.Api.vkBindBufferMemory(device.Handle, Handle, _gpuBufferMemory, 0);

    }

    public unsafe void Dispose()
    {
        _device.Api.vkDestroyBuffer(_device.Handle, Handle);
        _device.Api.vkFreeMemory(_device.Handle, _gpuBufferMemory, null);
    }

    internal void Upload()
    {

    }
}
