// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

using UOEngine.Runtime.Core;
using UOEngine.Runtime.Platform;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Vulkan;

[Service(UOEServiceLifetime.Singleton, typeof(IRenderer))]
public class VulkanRenderer : IRenderer
{
    private readonly IWindow _window;

    private VulkanDevice _device = null!;

    private readonly VulkanInstance _instance = new();

    private bool _enableDebug = false;
    private VulkanSwapchain _swapchain = null!;

    private readonly VulkanResourceFactory _resourceFactory;

    private uint _frameIndex = uint.MaxValue;

    private VulkanGraphicsContext? _graphicsContext;

    private VulkanGraphicsContext GraphicsContext => _graphicsContext ?? throw new InvalidOperationException("Graphics context not initialised");

    private struct PerFrameData
    {
        public VulkanFence SubmitFence;
        public VkSemaphore SwapchainAcquireSemaphore;
        public VkSemaphore SwapchainReleaseSemaphore;
        public VkCommandPool PrimaryCommandPool;
        public VkCommandBuffer PrimaryCommandBuffer;
    }

    private PerFrameData[] _perFrameData = [new(), new()];

    private ref PerFrameData GetCurrentFrameData() => ref _perFrameData[_frameIndex & 0x1];


    public VulkanRenderer(IWindow window, IRenderResourceFactory resourceFactory)
    {
        _window = window;

        if(CommandLine.HasOption("-debugdevice"))
        {
            _enableDebug = true;
        }

        _resourceFactory = (VulkanResourceFactory)resourceFactory;
    }

    public unsafe void Startup()
    {
        VkResult result = vkInitialize();

        result.CheckResult();

        _instance.Create("UOEngineApp", _enableDebug);

        _device = new VulkanDevice(_instance.GetSuitableDevice());

        _device.InitGpu(_instance.Api);

        _resourceFactory.SetDevice(_device);

        _swapchain = new VulkanSwapchain(_instance, _device);

        _swapchain.Create(_window.Handle);

        for(int i = 0; i < 2; i++)
        {
            ref var frameData = ref _perFrameData[i];

            frameData.SubmitFence = new VulkanFence(_device, true);
            frameData.SwapchainAcquireSemaphore = _device.CreateSemaphore();
            frameData.SwapchainReleaseSemaphore = _device.CreateSemaphore();

            VkCommandPoolCreateInfo poolCreateInfo = new()
            {
                flags = VkCommandPoolCreateFlags.Transient,
                queueFamilyIndex = _device.PresentQueue.FamilyIndex
            };

            _device.Api.vkCreateCommandPool(_device.Handle, &poolCreateInfo, null, out frameData.PrimaryCommandPool).CheckResult();
            _device.Api.vkAllocateCommandBuffer(_device.Handle, frameData.PrimaryCommandPool, out frameData.PrimaryCommandBuffer).CheckResult();

        }

        _graphicsContext = new VulkanGraphicsContext(_device);

    }

    public void Shutdown()
    {
        _instance.Destroy();
    }

    public void FrameBegin()
    {
        _frameIndex++;

        ref var frameData = ref GetCurrentFrameData();

        // Is the previous frame finished?
        frameData.SubmitFence.WaitForThenReset();

        _device.Api.vkResetCommandPool(_device.Handle, frameData.PrimaryCommandPool, VkCommandPoolResetFlags.None);

        AcquireNextImage();

        GraphicsContext.BeginRecording(frameData.PrimaryCommandBuffer);

        GraphicsContext.TransitionImageLayout(_swapchain.BackbufferToRenderInto.Image, VkImageLayout.Undefined, VkImageLayout.ColorAttachmentOptimal,
            0, VK_ACCESS_2_COLOR_ATTACHMENT_WRITE_BIT, VK_PIPELINE_STAGE_2_TOP_OF_PIPE_BIT, VK_PIPELINE_STAGE_2_COLOR_ATTACHMENT_OUTPUT_BIT);
    }

    public unsafe void FrameEnd()
    {
        ref var frameData = ref GetCurrentFrameData();

        GraphicsContext.TransitionImageLayout(_swapchain.BackbufferToRenderInto.Image, VkImageLayout.ColorAttachmentOptimal,
            VkImageLayout.PresentSrcKHR,
            VK_ACCESS_2_COLOR_ATTACHMENT_WRITE_BIT,                 // srcAccessMask
            0,                                                      // dstAccessMask
            VK_PIPELINE_STAGE_2_COLOR_ATTACHMENT_OUTPUT_BIT,        // srcStage
            VK_PIPELINE_STAGE_2_BOTTOM_OF_PIPE_BIT                  // dstStage
        );

        GraphicsContext.EndRecording();

        VkCommandBuffer commandBuffer = frameData.PrimaryCommandBuffer;
        VkPipelineStageFlags wait_stage = VkPipelineStageFlags.ColorAttachmentOutput;
        VkSemaphore waitSemaphore = frameData.SwapchainAcquireSemaphore;
        VkSemaphore signalSemaphore = frameData.SwapchainReleaseSemaphore;

        _device.PresentQueue.Submit(new VkSubmitInfo
        {
            commandBufferCount = 1,
            pCommandBuffers = &commandBuffer,
            waitSemaphoreCount = 1u,
            pWaitSemaphores = &waitSemaphore,
            pWaitDstStageMask = &wait_stage,
            signalSemaphoreCount = 1u,
            pSignalSemaphores = &signalSemaphore

        }, frameData.SubmitFence.Handle);

        _swapchain.Present(frameData.SwapchainReleaseSemaphore);
    }

    public IRenderContext CreateRenderContext()
    {
        return GraphicsContext;
    }

    public RhiRenderTarget GetViewportRenderTarget()
    {
        var renderTarget = new RhiRenderTarget();

        renderTarget.Setup(_swapchain.BackbufferToRenderInto);

        return renderTarget;
    }

    private void AcquireNextImage()
    {
        ref var frameData = ref GetCurrentFrameData();

        var result = _swapchain.AcquireNextImage(frameData.SwapchainAcquireSemaphore);

        if ((result == VkResult.SuboptimalKHR) || (result == VkResult.ErrorOutOfDateKHR))
        {
            _device.WaitForGpuIdle();
            _swapchain.Resize();
        }

    }

}
