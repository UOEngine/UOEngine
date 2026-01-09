// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.

namespace UOEngine.Runtime.Vulkan;

internal class VulkanUniformBuffer
{
    private VulkanMemoryAllocation _allocation;

    internal VulkanUniformBuffer(VulkanDevice device, uint size)
    {
        device.MemoryManager.AllocateUniformBuffer(size, out _allocation);
    }
}
