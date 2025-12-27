// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

[DebuggerDisplay("{_type.ToString()}")]
internal class VulkanQueue
{
    private readonly VkQueue _queue;
    private readonly VulkanQueueType _type;

    public VulkanQueue(VulkanQueueType type, VkQueue queue)
    {
        _queue = queue;
        _type = type;
    }
}
