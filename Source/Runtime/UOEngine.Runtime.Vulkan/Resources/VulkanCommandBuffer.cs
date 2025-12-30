// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Core;
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace UOEngine.Runtime.Vulkan;

internal class VulkanCommandBuffer
{
    public readonly VkCommandBuffer Handle;
    public readonly VulkanCommandBufferPool CommandBufferPool;

    public readonly VulkanFence Fence;

    public readonly string Name;

    private readonly VulkanDevice _device;

    private bool _isRecording;

    private static int _count = 0;


    internal unsafe VulkanCommandBuffer(VulkanDevice device, VulkanCommandBufferPool commandPool)
    {
        _device = device;
        CommandBufferPool = commandPool;

        VkCommandBufferAllocateInfo commandBufferAllocateInfo = new()
        {
            commandBufferCount = 1,
            commandPool = commandPool.Handle
        };

        _device.Api.vkAllocateCommandBuffer(_device.Handle, &commandBufferAllocateInfo, out Handle);

        Fence = new VulkanFence(device);

        Name = $"VulkanCommandBuffer{_count++}";

    }

    internal unsafe void CmdCopyBufferToImage(VkBuffer source, VkImage image, in VkBufferImageCopy bufferImageCopy)
    {
        var copyInfo = bufferImageCopy;

        _device.Api.vkCmdCopyBufferToImage(Handle, source, image, VkImageLayout.TransferDstOptimal, 1, &copyInfo);
    }

    internal unsafe void BeginRecording()
    {
        VkCommandBufferBeginInfo beginInfo = new()
        {
            flags = VkCommandBufferUsageFlags.OneTimeSubmit
        };

        _device.Api.vkBeginCommandBuffer(Handle, &beginInfo);
        _isRecording = true;
    }

    internal void EndRecording()
    {
        _device.Api.vkEndCommandBuffer(Handle);
        _isRecording = false;
    }

    internal unsafe void TransitionImageLayout(VkImage image, VkImageLayout oldLayout, VkImageLayout newLayout, 
        uint sourceQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED, uint destinationQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED)
    {
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
}
