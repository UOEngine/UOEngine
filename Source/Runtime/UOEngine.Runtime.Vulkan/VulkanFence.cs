// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;
using UOEngine.Runtime.Core;
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

[DebuggerDisplay("VulkanFence ({Name})")]
internal class VulkanFence
{
    public uint FrameSubmitted;
    public uint SignalCount { get; private set; }

    public readonly VkFence Handle;
    private readonly VulkanDevice _device;

    public readonly string Name;

    private bool _isSignaled;
    private static int _count = 0;

    public VulkanFence(VulkanDevice device, bool createSignaled = false)
    {
        _device = device;

        VkFenceCreateFlags flags = createSignaled? VkFenceCreateFlags.Signaled: VkFenceCreateFlags.None;

        _device.Api.vkCreateFence(device.Handle, flags, out Handle).CheckResult();

        _isSignaled = createSignaled;

        Name = $"Fence{_count++}";
    }
    
    public void WaitForThenReset()
    {
        Wait();
        Reset();
    }

    public void Wait()
    {
        if (_isSignaled)
        {
            return;
        }

        _device.Api.vkWaitForFences(_device.Handle, Handle, true, ulong.MaxValue);

        SignalCount++;
        _isSignaled = true;
    }

    public void Reset()
    {
        if(_isSignaled == false)
        {
            return;
        }

        _device.Api.vkResetFences(_device.Handle, Handle);
        _isSignaled = false;
    }

    internal bool IsSignaled()
    {
        if(_isSignaled)
        {
            return true;
        }

        var result = _device.Api.vkGetFenceStatus(_device.Handle, Handle);

        switch(result)
        {
            case VkResult.Success:
                {
                    _isSignaled = true;
                    SignalCount++;
                    return true;
                }

            case VkResult.NotReady:
                {
                    return false;
                }

            default:
                throw new NotImplementedException();
        }
    }

}
