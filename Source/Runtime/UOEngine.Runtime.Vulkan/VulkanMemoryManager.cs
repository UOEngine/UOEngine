// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

struct VulkanMemoryAllocation
{

}

internal class VulkanMemoryManager
{
    internal void Init()
    {

    }

    internal bool AllocateUniformBuffer(uint size, out VulkanMemoryAllocation memoryAllocation)
    {
        return AllocateBuffer(size, VkBufferUsageFlags.UniformBuffer, VkMemoryPropertyFlags.HostCoherent | VkMemoryPropertyFlags.HostVisible, out memoryAllocation);
    }

    internal bool AllocateBuffer(uint size, VkBufferUsageFlags usage, VkMemoryPropertyFlags memoryProperties, out VulkanMemoryAllocation memoryAllocation)
    {
        return false;
    }
}
