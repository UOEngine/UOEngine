// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.RHI;
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace UOEngine.Runtime.Vulkan;

internal enum VulkanCommandBufferState
{
    Initial,
    Recording,
    Executable,
    Submitted
}

internal class VulkanCommandBuffer
{
    public readonly VkCommandBuffer Handle;
    public readonly VulkanCommandBufferPool CommandBufferPool;

    public VulkanCommandBufferState State { get; private set; }

    public VulkanFence Fence { get; private set; }

    public readonly string Name;

    public readonly bool IsUploadOnly;

    private readonly VulkanDevice _device;

    private static int _count = 0;

    internal unsafe VulkanCommandBuffer(VulkanDevice device, VulkanCommandBufferPool commandPool, string? name = null)
    {
        _device = device;
        CommandBufferPool = commandPool;

        VkCommandBufferAllocateInfo commandBufferAllocateInfo = new()
        {
            commandBufferCount = 1,
            commandPool = commandPool.Handle,
            level = VkCommandBufferLevel.Primary
        };

        _device.Api.vkAllocateCommandBuffer(_device.Handle, &commandBufferAllocateInfo, out Handle);

        Fence = new VulkanFence(device);

        State = VulkanCommandBufferState.Initial;

        Name = name ?? $"VulkanCommandBuffer{_count++}";
    }

    internal unsafe void CmdCopyBufferToImage(VkBuffer source, VulkanTexture destinationTexture, in VkBufferImageCopy bufferImageCopy)
    {
        EnsureState(destinationTexture, RhiRenderTextureUsage.CopyDestination);

        var copyInfo = bufferImageCopy;

        _device.Api.vkCmdCopyBufferToImage(Handle, source, destinationTexture.Image, VkImageLayout.TransferDstOptimal, 1, &copyInfo);
    }

    internal unsafe void CmdCopyBuffer(VkBuffer sourceBuffer, VkBuffer destinationBuffer, VkBufferCopy bufferCopy)
    {
        var copyInfo = bufferCopy;

        _device.Api.vkCmdCopyBuffer(Handle, sourceBuffer, destinationBuffer, 1, &copyInfo);
    }

    internal unsafe void BeginRecording()
    {
        //Debug.WriteLine($"VulkanCommandBuffer.BeginRecording: {Name}");

        VkCommandBufferBeginInfo beginInfo = new()
        {
            flags = VkCommandBufferUsageFlags.OneTimeSubmit
        };

        _device.Api.vkResetCommandBuffer(Handle, VkCommandBufferResetFlags.None);
        _device.Api.vkBeginCommandBuffer(Handle, &beginInfo);

        State = VulkanCommandBufferState.Recording;
    }

    internal void EndRecording()
    {
        _device.Api.vkEndCommandBuffer(Handle);

        State = VulkanCommandBufferState.Executable;
    }

    internal unsafe void EnsureState(VulkanTexture texture, RhiRenderTextureUsage desiredUsage)
    {
        ref var current = ref texture.State;
        var target = desiredUsage.ToVkTextureState(0);

        if (current.Layout == target.Layout &&
            current.AccessMask == target.AccessMask &&
            current.StageMask == target.StageMask &&
            current.QueueFamilyIndex == target.QueueFamilyIndex)
        {
            return;
        }

        VkImageMemoryBarrier2 barrier = new()
        {
            oldLayout = current.Layout,
            newLayout = target.Layout,
            srcAccessMask = current.AccessMask,
            dstAccessMask = target.AccessMask,
            srcStageMask = current.StageMask,
            dstStageMask = target.StageMask,
            srcQueueFamilyIndex = current.QueueFamilyIndex,
            dstQueueFamilyIndex = target.QueueFamilyIndex,
            image = texture.Image,
            subresourceRange = new VkImageSubresourceRange(VK_IMAGE_ASPECT_COLOR_BIT, 0, 1, 0, 1)
        };

        if (current.Layout == VkImageLayout.Undefined)
        {
            barrier.srcAccessMask = VkAccessFlags2.None;
            barrier.srcStageMask = VkPipelineStageFlags2.None;
        }

        VkDependencyInfo dependencyInfo = new()
        {
            imageMemoryBarrierCount = 1,
            pImageMemoryBarriers = &barrier
        };

        _device.Api.vkCmdPipelineBarrier2(Handle, &dependencyInfo);

        current = target;
    }

    internal unsafe void TransitionImageLayout(VkImage image, VkImageLayout oldLayout, VkImageLayout newLayout, 
        uint sourceQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED, uint destinationQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED)
    {
        UOEDebug.Assert(false, "Do not use!");

        // Initialize the VkImageMemoryBarrier2 structure
        VkImageMemoryBarrier2 imageBarrier = new VkImageMemoryBarrier2
        {
            // Specify the old and new layouts of the image
            oldLayout = oldLayout,        // Current layout of the image
            newLayout = newLayout,        // Target layout of the image

            srcQueueFamilyIndex = sourceQueueFamilyIndex,
            dstQueueFamilyIndex = destinationQueueFamilyIndex,

            // Specify the image to be affected by this barrier
            image = image,

            // Define the subresource range (which parts of the image are affected)
            subresourceRange = new VkImageSubresourceRange(VK_IMAGE_ASPECT_COLOR_BIT, 0, 1, 0, 1)
        };

        if(oldLayout == VkImageLayout.Undefined && newLayout == VkImageLayout.TransferDstOptimal)
        {
            imageBarrier.srcAccessMask = 0;
            imageBarrier.srcStageMask = VkPipelineStageFlags2.TopOfPipe;

            imageBarrier.dstAccessMask = VkAccessFlags2.TransferWrite;
            imageBarrier.dstStageMask = VkPipelineStageFlags2.Transfer;
        }
        else if (oldLayout == VkImageLayout.PresentSrcKHR && newLayout == VkImageLayout.ColorAttachmentOptimal)
        {
            imageBarrier.srcAccessMask = VkAccessFlags2.None;
            imageBarrier.srcStageMask = VkPipelineStageFlags2.None;

            imageBarrier.dstAccessMask = VkAccessFlags2.ColorAttachmentWrite;
            imageBarrier.dstStageMask = VkPipelineStageFlags2.ColorAttachmentOutput;
        }
        else if (oldLayout == VkImageLayout.Undefined && newLayout == VkImageLayout.ColorAttachmentOptimal)
        {
            imageBarrier.srcAccessMask = VkAccessFlags2.None;
            imageBarrier.srcStageMask = VkPipelineStageFlags2.None;

            imageBarrier.dstAccessMask = VkAccessFlags2.ColorAttachmentWrite;
            imageBarrier.dstStageMask = VkPipelineStageFlags2.ColorAttachmentOutput;
        }
        else if (oldLayout == VkImageLayout.ColorAttachmentOptimal && newLayout == VkImageLayout.PresentSrcKHR)
        {
            imageBarrier.srcAccessMask = VkAccessFlags2.ColorAttachmentWrite;
            imageBarrier.srcStageMask = VkPipelineStageFlags2.ColorAttachmentOutput;

            imageBarrier.dstAccessMask = VkAccessFlags2.None;
            imageBarrier.dstStageMask = VkPipelineStageFlags2.None;
        }
        else if (oldLayout == VkImageLayout.TransferDstOptimal && newLayout == VkImageLayout.ShaderReadOnlyOptimal)
        {
            imageBarrier.srcAccessMask = VkAccessFlags2.TransferWrite;
            imageBarrier.srcStageMask = VkPipelineStageFlags2.Transfer;

            imageBarrier.dstAccessMask = VkAccessFlags2.ShaderRead;
            imageBarrier.dstStageMask = VkPipelineStageFlags2.FragmentShader;
        }
        else
        {
            UOEDebug.NotImplemented(); // not handed layout transition.
        }

        // Initialize the VkDependencyInfo structure
        VkDependencyInfo dependencyInfo = new()
        {
            dependencyFlags = 0,                    // No special dependency flags
            imageMemoryBarrierCount = 1,                    // Number of image memory barriers
            pImageMemoryBarriers = &imageBarrier        // Pointer to the image memory barrier(s)
        };

        // Record the pipeline barrier into the command buffer
        _device.Api.vkCmdPipelineBarrier2(Handle, &dependencyInfo);
    }

    internal void MarkSubmitted()
    {
        //Fence = fence;
        State = VulkanCommandBufferState.Submitted;
    }

    internal void MarkFinished() => State = VulkanCommandBufferState.Initial;
}
