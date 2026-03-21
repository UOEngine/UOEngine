// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;
using UOEngine.Runtime.Core;
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

[DebuggerDisplay("{_type.ToString()} Queue")]
public class VulkanQueue: IDisposable
{
    public readonly VkQueue Handle;
    public readonly uint FamilyIndex;

    private readonly VulkanQueueType _type;
    private readonly VulkanDevice _device;

    private VulkanCommandBuffer? _lastCommandBufferSubmitted;

    //private VulkanFence _submissionFence;

    struct AllocationInfo
    {
        public VulkanCommandBufferPool CommandBufferPool;
        public VulkanCommandBuffer CommandBuffer;
    }

    private List<AllocationInfo> _freeCommandBufferAllocators = [];

    private List<VulkanCommandBufferPool> _commandBufferPools = [];

    private readonly int _maxCommandBufferPools = 8;


    public VulkanQueue(VulkanDevice device, VulkanQueueType type, VkQueue queue)
    {
        _device = device;
        Handle = queue;
        _type = type;
        FamilyIndex = (uint)type;

        VulkanDebug.SetDebugName(Handle, _type.ToString());
    }

    internal unsafe void Submit(VulkanCommandBuffer commandBuffer)
    {
        VkCommandBuffer buffer = commandBuffer.Handle;

        VkSubmitInfo submitInfo = new()
        {
            commandBufferCount = 1,
            pCommandBuffers = &buffer
        };

        _device.Api.vkQueueSubmit(Handle, submitInfo, commandBuffer.Fence.Handle);

        AllocationInfo info = new()
        {
            CommandBuffer = commandBuffer,
            CommandBufferPool = commandBuffer.CommandBufferPool
        };

        _freeCommandBufferAllocators.Add(info);

    }

    internal void Submit(VulkanCommandBuffer commandBuffer, in VkSubmitInfo submitInfo, VulkanFence fence)
    {
        AllocationInfo info = new()
        {
            CommandBuffer = commandBuffer,
            CommandBufferPool = commandBuffer.CommandBufferPool
        };

        UOEDebug.Assert(fence.IsSignaled == false);

        Debug.WriteLine($"Submitting {commandBuffer.Name} with fence {fence.Name}");

        _device.Api.vkQueueSubmit(Handle, submitInfo, fence.Handle).CheckResult();

        commandBuffer.MarkSubmitted();

        _lastCommandBufferSubmitted = commandBuffer;

        _freeCommandBufferAllocators.Add(info);
    }

    internal void Submit(ReadOnlySpan<VulkanGraphicsContext> contexts, Span<VkSubmitInfo> submitInfos, VulkanFence fence)
    {
        UOEDebug.Assert(fence.IsSignaled == false);

        _device.Api.vkQueueSubmit(Handle, submitInfos, fence.Handle).CheckResult();

        foreach (var context in contexts)
        {
            context.CommandBuffer.MarkSubmitted();

            AllocationInfo info = new()
            {
                CommandBuffer = context.CommandBuffer,
                CommandBufferPool = context.CommandBuffer.CommandBufferPool
            };

            _freeCommandBufferAllocators.Add(info);
        }
    }


    internal VulkanCommandBuffer CreateCommandBuffer()
    {
        int freeCommandBufferIndex = -1;

        for(int i = 0; i < _freeCommandBufferAllocators.Count; i++)
        {
            _freeCommandBufferAllocators[i].CommandBuffer.Fence.Refresh();

            if (_freeCommandBufferAllocators[i].CommandBuffer.Fence.IsSignaled)
            {
                freeCommandBufferIndex = i;
                break;
            }
        }

        VulkanCommandBuffer commandBuffer = null!;
        VulkanCommandBufferPool commandBufferPool = null!;

        if (freeCommandBufferIndex >= 0)
        {
            commandBuffer = _freeCommandBufferAllocators[freeCommandBufferIndex].CommandBuffer;
            commandBufferPool = _freeCommandBufferAllocators[freeCommandBufferIndex].CommandBufferPool;

            //commandBufferPool.Reset(); // ??

            _freeCommandBufferAllocators.RemoveAt(freeCommandBufferIndex);
        }
        else
        {
            UOEDebug.Assert(_commandBufferPools.Count <= _maxCommandBufferPools);

            commandBufferPool = new VulkanCommandBufferPool(_device, FamilyIndex);

            _commandBufferPools.Add(commandBufferPool);

            commandBuffer = commandBufferPool.Create();
        }

        commandBuffer.Fence.Reset();

        commandBuffer.BeginRecording();

        return commandBuffer;

    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
