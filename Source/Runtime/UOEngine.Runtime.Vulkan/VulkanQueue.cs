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

    private VulkanCommandBuffer? _lastCommandBufferSubmitted;

    private VulkanFence? _submissionFence;

    private List<VulkanCommandBufferPool> _commandBufferPools = [];

    private List<VulkanCommandBufferPool> _submittedCommandBufferPools = [];

    private VulkanCommandBufferPool ActiveCommandBufferPool => _activeCommandBufferPool ?? throw new InvalidOperationException();

    private VulkanCommandBufferPool? _activeCommandBufferPool;

    private readonly int _maxCommandBufferPools = 8;

    public VulkanQueue(VulkanDevice device, VulkanQueueType type, VkQueue queue)
    {
        _device = device;
        Handle = queue;
        _type = type;
        FamilyIndex = (uint)type;

        VulkanDebug.SetDebugName(Handle, _type.ToString());

        UploadContext = new VulkanUploadContext(device, FamilyIndex);
    }

    internal unsafe void SubmitUpload(VulkanCommandBuffer commandBuffer, VulkanFence fence)
    {
        VkCommandBuffer buffer = commandBuffer.Handle;

        VkSubmitInfo submitInfo = new()
        {
            commandBufferCount = 1,
            pCommandBuffers = &buffer
        };

        _device.Api.vkQueueSubmit(Handle, submitInfo, fence.Handle);
    }

    internal unsafe void Submit(VulkanCommandBuffer commandBuffer, VulkanFence fence)
    {
        VkCommandBuffer buffer = commandBuffer.Handle;

        VkSubmitInfo submitInfo = new()
        {
            commandBufferCount = 1,
            pCommandBuffers = &buffer
        };

        _device.Api.vkQueueSubmit(Handle, submitInfo, fence.Handle);

        //AllocationInfo info = new()
        //{
        //    CommandBuffer = commandBuffer,
        //    CommandBufferPool = commandBuffer.CommandBufferPool
        //};

        _submissionFence = fence;

        ActiveCommandBufferPool.SubmissionFence = fence;

        _submittedCommandBufferPools.Add(ActiveCommandBufferPool);

        _activeCommandBufferPool = null;

    }

    //internal void Submit(VulkanCommandBuffer commandBuffer, in VkSubmitInfo submitInfo, VulkanFence fence)
    //{
    //    AllocationInfo info = new()
    //    {
    //        CommandBuffer = commandBuffer,
    //        CommandBufferPool = commandBuffer.CommandBufferPool
    //    };

    //    UOEDebug.Assert(fence.IsSignaled == false);

    //    Debug.WriteLine($"Submitting {commandBuffer.Name} with fence {fence.Name}");

    //    _device.Api.vkQueueSubmit(Handle, submitInfo, fence.Handle).CheckResult();

    //    commandBuffer.MarkSubmitted();

    //    _lastCommandBufferSubmitted = commandBuffer;

    //    _freeCommandBufferAllocators.Add(info);
    //}

    internal VulkanFence Submit(Span<VkSubmitInfo> submitInfos, VulkanFence fence)
    {
        UOEDebug.Assert(fence.IsSignaled == false);

        _device.Api.vkQueueSubmit(Handle, submitInfos, fence.Handle).CheckResult();

        _submissionFence = fence;

        ActiveCommandBufferPool.SubmissionFence = fence;

        _submittedCommandBufferPools.Add(ActiveCommandBufferPool);

        _activeCommandBufferPool = null;

        return fence;
    }

    internal VulkanCommandBuffer CreateCommandBuffer()
    {
        if(_activeCommandBufferPool == null)
        {
            for (int i = 0; i < _submittedCommandBufferPools.Count; i++)
            {
                var pool = _submittedCommandBufferPools[i];
                var fence = pool.SubmissionFence;

                if (fence == null)
                {
                    pool.Reset();

                    _activeCommandBufferPool = pool;
                    _submittedCommandBufferPools.RemoveAt(i);

                    break;
                }

                fence.Refresh();

                if (fence.IsSignaled)
                {
                    fence.Reset();
                    pool.Reset();

                    pool.SubmissionFence = null;

                    _activeCommandBufferPool = pool;

                    _submittedCommandBufferPools.RemoveAt(i);

                    break;
                }
            }
        }

        if(_activeCommandBufferPool == null)
        {
            UOEDebug.Assert(_commandBufferPools.Count <= _maxCommandBufferPools);

            _activeCommandBufferPool = new VulkanCommandBufferPool(_device, FamilyIndex);

            _commandBufferPools.Add(_activeCommandBufferPool);

        }

        VulkanCommandBuffer commandBuffer = _activeCommandBufferPool.Create();

        commandBuffer.BeginRecording();

        return commandBuffer;

    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
