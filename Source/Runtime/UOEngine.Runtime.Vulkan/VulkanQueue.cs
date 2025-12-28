// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

[DebuggerDisplay("{_type.ToString()} Queue")]
public class VulkanQueue
{
    public readonly VkQueue Handle;
    public readonly uint FamilyIndex;

    private readonly VulkanQueueType _type;
    private readonly VulkanDevice _device;

    public VulkanQueue(VulkanDevice device, VulkanQueueType type, VkQueue queue)
    {
        _device = device;
        Handle = queue;
        _type = type;
        FamilyIndex = (uint)type;
    }

    public void Submit(in VkSubmitInfo submitInfo, VkFence submissionFence)
    {
        _device.Api.vkQueueSubmit(Handle, submitInfo, submissionFence);
    }
}
