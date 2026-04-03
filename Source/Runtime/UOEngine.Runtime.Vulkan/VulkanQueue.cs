// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;
using UOEngine.Runtime.Core;
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

[DebuggerDisplay("{_type.ToString()} Queue")]
internal class VulkanQueue: IDisposable
{
    public readonly VkQueue Handle;
    public readonly uint FamilyIndex;

    internal readonly VulkanUploadContext UploadContext;

    private readonly VulkanQueueType _type;
    private readonly VulkanDevice _device;

    private List<VulkanCommandBufferPool> _commandBufferPools = [];

    private List<VulkanCommandBufferPool> _freeCommandBufferPools = [];

    private readonly int _maxCommandBufferPools = 16;

    public VulkanQueue(VulkanDevice device, VulkanQueueType type, VkQueue queue)
    {
        _device = device;
        Handle = queue;
        _type = type;
        FamilyIndex = (uint)type;

        VulkanDebug.SetDebugName(Handle, _type.ToString());

        UploadContext = new VulkanUploadContext(device, FamilyIndex);
    }

    internal unsafe void Submit(VulkanCommandBuffer commandBuffer, VulkanFence? fence)
    {
        UOEDebug.Assert(UOEThread.IsMainThread);

        VkCommandBuffer buffer = commandBuffer.Handle;

        VkSubmitInfo submitInfo = new()
        {
            commandBufferCount = 1,
            pCommandBuffers = &buffer
        };

        var vkFence = fence?.Handle ?? VkFence.Null;

        _device.Api.vkQueueSubmit(Handle, submitInfo, vkFence);
    }

    internal VulkanFence Submit(Span<VkSubmitInfo> submitInfos, VulkanFence fence)
    {
        UOEDebug.Assert(UOEThread.IsMainThread);

       _device.Api.vkQueueSubmit(Handle, submitInfos, fence.Handle).CheckResult();

        return fence;
    }

    internal VkResult Present(VkSemaphore semaphore, VkSwapchainKHR swapchain, uint nextImageIndex)
    {
        UOEDebug.Assert(UOEThread.IsMainThread);
 
        return _device.Api.vkQueuePresentKHR(Handle, semaphore, swapchain, nextImageIndex);
    }

    internal VulkanCommandBufferPool AllocatePool()
    {
        var pool = new VulkanCommandBufferPool(_device, FamilyIndex);

        _commandBufferPools.Add(pool);

        return pool;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
