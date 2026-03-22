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

    private VulkanCommandBufferPool ActiveCommandBufferPool => _activeCommandBufferPool ?? throw new InvalidOperationException();

    private VulkanCommandBufferPool? _activeCommandBufferPool;

    private readonly int _maxCommandBufferPools = 16;

    private struct SubmittedPool
    {
        public VulkanCommandBufferPool Pool;
        public VulkanFence Fence;
        public uint ExpectedSignalCount;
    }

    private List<SubmittedPool> _submittedPools = [];

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

        _submittedPools.Add(new SubmittedPool
        {
            Pool = _activeCommandBufferPool!,
            Fence = fence,
            ExpectedSignalCount = fence.SignalCount + 1
        });

        _activeCommandBufferPool = null;
    }


    internal VulkanFence Submit(Span<VkSubmitInfo> submitInfos, VulkanFence fence)
    {
        UOEDebug.Assert(fence.IsSignaled == false);

        _device.Api.vkQueueSubmit(Handle, submitInfos, fence.Handle).CheckResult();

        _submittedPools.Add(new SubmittedPool
        {
            Pool = _activeCommandBufferPool!,
            Fence = fence,
            ExpectedSignalCount = fence.SignalCount + 1
        });

        _activeCommandBufferPool = null;

        return fence;
    }

    internal VulkanCommandBuffer CreateCommandBuffer()
    {
        if(_activeCommandBufferPool == null)
        {
            for (int i = 0; i < _submittedPools.Count; i++)
            {
                var submitted = _submittedPools[i];

                if(submitted.Fence.SignalCount >= submitted.ExpectedSignalCount)
                {
                    submitted.Pool.Reset();

                    _activeCommandBufferPool = submitted.Pool;

                    _submittedPools.RemoveAt(i);

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
