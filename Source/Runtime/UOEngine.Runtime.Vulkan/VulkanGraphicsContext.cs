// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Vulkan
{
    internal class VulkanGraphicsContext : IRenderContext
    {
        public ShaderInstance ShaderInstance { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IRhiIndexBuffer IndexBuffer { set => throw new NotImplementedException(); }
        public IRhiVertexBuffer VertexBuffer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public RhiSampler Sampler { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private readonly VulkanDevice _device;

        private VkCommandBuffer _commandBuffer;

        public VulkanGraphicsContext(VulkanDevice device)
        {
            _device = device;
        }

        public void BeginRecording(VkCommandBuffer commandBuffer)
        {
            _commandBuffer = commandBuffer;

            VkCommandBufferBeginInfo beginInfo = new()
            {
                flags = VkCommandBufferUsageFlags.OneTimeSubmit
            };

            unsafe
            {
                _device.Api.vkBeginCommandBuffer(commandBuffer, &beginInfo).CheckResult();
            }
        }

        public void BeginRenderPass(in RenderPassInfo renderPassInfo)
        {
            VkRenderPassBeginInfo renderPassBeginInfo = new()
            {

            };

            unsafe
            {
                //_device.Api.vkCmdBeginRenderPass(_commandBuffer, &renderPassBeginInfo, VkSubpassContents.Inline);
            }
        }

        public void DrawIndexedPrimitives(uint numIndices, uint numInstances, uint firstIndex, uint vertexOffset, uint firstInstance)
        {

        }

        public void EndRecording()
        {
            _device.Api.vkEndCommandBuffer(_commandBuffer).CheckResult();
        }

        public void EndRenderPass()
        {

        }

        public void SetGraphicsPipeline(in RhiGraphicsPipelineDescription graphicsPipelineDescription)
        {

        }

        public void WaitForGpuIdle()
        {
            _device.WaitForGpuIdle();
        }

        public unsafe void TransitionImageLayout(VkImage image, VkImageLayout oldLayout,VkImageLayout newLayout, VkAccessFlags2 srcAccessMask,
                                         VkAccessFlags2 dstAccessMask, VkPipelineStageFlags2 srcStage, VkPipelineStageFlags2 dstStage)
        {
            // Initialize the VkImageMemoryBarrier2 structure
            VkImageMemoryBarrier2 imageBarrier = new VkImageMemoryBarrier2
            {
                // Specify the pipeline stages and access masks for the barrier
                srcStageMask = srcStage,             // Source pipeline stage mask
                srcAccessMask = srcAccessMask,        // Source access mask
                dstStageMask = dstStage,             // Destination pipeline stage mask
                dstAccessMask = dstAccessMask,        // Destination access mask

                // Specify the old and new layouts of the image
                oldLayout = oldLayout,        // Current layout of the image
                newLayout = newLayout,        // Target layout of the image

                // We are not changing the ownership between queues
                srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED,
                dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED,

                // Specify the image to be affected by this barrier
                image = image,

                // Define the subresource range (which parts of the image are affected)
                subresourceRange = new VkImageSubresourceRange(VK_IMAGE_ASPECT_COLOR_BIT, 0, 1, 0, 1)
            };

            // Initialize the VkDependencyInfo structure
            VkDependencyInfo dependencyInfo = new()
            {
                dependencyFlags = 0,                    // No special dependency flags
                imageMemoryBarrierCount = 1,                    // Number of image memory barriers
                pImageMemoryBarriers = &imageBarrier        // Pointer to the image memory barrier(s)
            };

            // Record the pipeline barrier into the command buffer
            _device.Api.vkCmdPipelineBarrier2(_commandBuffer, &dependencyInfo);
        }
    }
}
