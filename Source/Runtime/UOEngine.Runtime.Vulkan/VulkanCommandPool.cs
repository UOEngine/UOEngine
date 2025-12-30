// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

internal class VulkanCommandBufferPool
{
    internal readonly VkCommandPool Handle;
    
    private readonly uint _queueIndex;
    private readonly VulkanDevice _device;

    internal unsafe VulkanCommandBufferPool(VulkanDevice device, uint queueFamilyIndex)
    {
        _device = device;

        VkCommandPoolCreateInfo commandPoolCreateInfo = new()
        {
            queueFamilyIndex = queueFamilyIndex,
        };

        _device.Api.vkCreateCommandPool(_device.Handle, &commandPoolCreateInfo, out var commandPool);

        Handle = commandPool;
    }
    internal void Reset() => _device.Api.vkResetCommandPool(_device.Handle, Handle, VkCommandPoolResetFlags.None);
}
