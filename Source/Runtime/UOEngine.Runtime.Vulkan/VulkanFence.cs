// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;

using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

[DebuggerDisplay("VulkanFence ({Name})")]
internal class VulkanFence
{
    public uint FrameSubmitted = uint.MaxValue;
    public uint SignalCount { get; private set; }

    public bool IsSignaled { get; set; }

    public readonly VkFence Handle;
    private readonly VulkanDevice _device;

    public readonly string Name;

    private static int _count = 0;

    internal VulkanFence(VulkanDevice device, bool createSignaled, string? name = null)
    {
        _device = device;

        VkFenceCreateFlags flags = createSignaled? VkFenceCreateFlags.Signaled: VkFenceCreateFlags.None;

        _device.Api.vkCreateFence(device.Handle, flags, out Handle).CheckResult();

        IsSignaled = createSignaled;

        Name = $"Fence{_count++}";

        if(name != null)
        {
            Name = name;
        }

        VulkanDebug.SetDebugName(Handle, Name);
    }

    public void WaitForThenReset()
    {
        Wait();
        Reset();
    }

    public void Wait()
    {
        if (IsSignaled)
        {
            return;
        }

        //Debug.WriteLine($"VulkanFence.Wait: {Name}");

        _device.Api.vkWaitForFences(_device.Handle, Handle, true, ulong.MaxValue);

        SignalCount++;
        IsSignaled = true;
    }

    public void Reset()
    {
        if (IsSignaled == false)
        {
            return;
        }

        _device.Api.vkResetFences(_device.Handle, Handle).CheckResult();
        IsSignaled = false;
    }

    internal void Refresh()
    {
        if (IsSignaled)
        {
            return;
        }

        var result = _device.Api.vkGetFenceStatus(_device.Handle, Handle);

        switch (result)
        {
            case VkResult.Success:
                {
                    IsSignaled = true;
                    SignalCount++;
                    break;
                }

            case VkResult.NotReady:
                {
                    break;
                }

            default:
                throw new NotImplementedException();
        }
    }
}
