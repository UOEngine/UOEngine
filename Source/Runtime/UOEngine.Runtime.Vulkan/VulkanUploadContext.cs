// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.

namespace UOEngine.Runtime.Vulkan;

internal class VulkanUploadContext
{

    internal bool InFlight { get; private set; }

    internal readonly VulkanFence _fence;

    private readonly  VulkanCommandBufferPool Pool;

    private readonly VulkanCommandBuffer _commandBuffer;
    private readonly VulkanDevice _device;

    internal VulkanUploadContext(VulkanDevice device, uint queueFamilyIndex)
    {
        _device = device;
        Pool = new VulkanCommandBufferPool(device, queueFamilyIndex);
        _commandBuffer = Pool.Create();
        _fence = new VulkanFence(device, false);
    }

    internal VulkanCommandBuffer GetCommandBuffer()
    {
        WaitForUpload();

        Pool.Reset();

        return _commandBuffer;
    }

    internal void Submit()
    {
        //_device.GraphicsQueue.SubmitUpload(_commandBuffer, _fence);
        //InFlight = true;
    }

    internal void WaitForUpload()
    {
        if (InFlight)
        {
            _fence.WaitForThenReset();
            InFlight = false;
        }
    }
}
