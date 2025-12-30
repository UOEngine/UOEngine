// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

internal class VulkanFence
{
    public readonly VkFence Handle;
    private readonly VulkanDevice _device;

    public VulkanFence(VulkanDevice device, bool createSignaled = false)
    {
        _device = device;

        VkFenceCreateFlags flags = createSignaled? VkFenceCreateFlags.Signaled: VkFenceCreateFlags.None;

        _device.Api.vkCreateFence(device.Handle, flags, out Handle).CheckResult();

    }
    
    public void WaitForThenReset()
    {
        Wait();
        Reset();
    }

    public void Wait() => _device.Api.vkWaitForFences(_device.Handle, Handle, true, ulong.MaxValue);

    public void Reset() => _device.Api.vkResetFences(_device.Handle, Handle);

    internal bool IsSignaled() => _device.Api.vkGetFenceStatus(_device.Handle, Handle) == VkResult.Success;

}
