// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

using UOEngine.Runtime.Core;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Vulkan;

internal class VulkanGraphicsContext : IRenderContext
{
    public ShaderInstance ShaderInstance { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IRhiIndexBuffer IndexBuffer { set => throw new NotImplementedException(); }
    public IRhiVertexBuffer VertexBuffer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public RhiSampler Sampler { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    internal VulkanCommandBuffer CommandBuffer => _commandBuffer ?? throw new InvalidOperationException("VulkanGraphicsContext: CommandBuffer not initialised");

    public bool IsInRenderPass { get; private set; }

    private readonly VulkanDevice _device;

    private VulkanCommandBuffer? _commandBuffer;

    public VulkanGraphicsContext(VulkanDevice device)
    {
        _device = device;
    }

    public void BeginRecording(VulkanCommandBuffer commandBuffer)
    {
        _commandBuffer = commandBuffer;

        //VkCommandBufferBeginInfo beginInfo = new()
        //{
        //    flags = VkCommandBufferUsageFlags.OneTimeSubmit
        //};

        //unsafe
        //{
        //    _device.Api.vkBeginCommandBuffer(CommandBuffer.Handle, &beginInfo).CheckResult();
        //}
    }

    public unsafe void BeginRenderPass(in RenderPassInfo renderPassInfo)
    {
        VulkanTexture texture = (VulkanTexture)renderPassInfo.RenderTarget.Texture;

        VkRenderingAttachmentInfo colorAttachment = new()
        {
            imageView = texture.ImageView,
            imageLayout = VkImageLayout.ColorAttachmentOptimal,
            resolveMode = VkResolveModeFlags.None,
            loadOp = VkAttachmentLoadOp.Clear,
            storeOp = VkAttachmentStoreOp.Store,
            clearValue = new(1.0f, 0.0f, 0.0f)  // Red to debug
        };

        VkRenderingInfo renderingInfo = new()
        {
            renderArea = new VkRect2D(VkOffset2D.Zero, new(texture.Width, texture.Height)),
            layerCount = 1u,
            colorAttachmentCount = 1,
            pColorAttachments = &colorAttachment,
        };

        _device.Api.vkCmdBeginRendering(CommandBuffer.Handle, &renderingInfo);

        IsInRenderPass = true;
    }

    public void DrawIndexedPrimitives(uint numIndices, uint numInstances, uint firstIndex, uint vertexOffset, uint firstInstance)
    {

    }

    public void EndRecording()
    {
        UOEDebug.Assert(IsInRenderPass == false);

        _device.Api.vkEndCommandBuffer(CommandBuffer.Handle).CheckResult();
    }

    public void EndRenderPass()
    {
        _device.Api.vkCmdEndRendering(CommandBuffer.Handle);

        IsInRenderPass = false;
    }

    public void SetGraphicsPipeline(in RhiGraphicsPipelineDescription graphicsPipelineDescription)
    {

    }

    public void WaitForGpuIdle()
    {
        _device.WaitForGpuIdle();
    }

    internal unsafe void TransitionImageLayout(VkImage image, VkImageLayout oldLayout,VkImageLayout newLayout)
    {
        CommandBuffer.TransitionImageLayout(image, oldLayout, newLayout);
    }

}
