// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

using UOEngine.Runtime.Core;
using UOEngine.Runtime.Platform;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.RHI;
using System.Diagnostics;

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

    private VulkanGlobalSamplers _globalSamplers = null!;

    private struct PerFrameData
    {
        public VulkanFence SubmitFence;
        public uint FenceSignalCount;
        public VkSemaphore SwapchainAcquireSemaphore;
        public VkSemaphore SwapchainReleaseSemaphore;
        public VulkanScratchBlockAllocator UniformBufferScratchAllocator;
        public VulkanDescriptorPool DescriptorPool;
        public VulkanCommandBuffer CommandBuffer;

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

        window.OnResized += (obj) =>
        {
            _device.WaitForGpuIdle();
            _swapchain.Resize();
        };
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
            frameData.UniformBufferScratchAllocator = new VulkanScratchBlockAllocator(_device);
            frameData.DescriptorPool = new VulkanDescriptorPool(_device);
            frameData.CommandBuffer = _device.GraphicsQueue.CreateCommandBuffer();
        }

        _globalSamplers = new VulkanGlobalSamplers(_device);
        _graphicsContext = new VulkanGraphicsContext(_device, _globalSamplers);

    }

    public void Shutdown()
    {
        _instance.Destroy();
    }

    public void FrameBegin()
    {
        _frameIndex++;

        //Debug.WriteLine($"VulkanRenderer.FrameBegin: {_frameIndex}");

        ref var frameData = ref GetCurrentFrameData();

        //_device.WaitForGpuIdle();
        // Is the previous frame finished?
        //if(frameData.FenceSignalCount <= frameData.SubmitFence?.SignalCount)
        {
            frameData.SubmitFence?.Refresh();
            frameData.SubmitFence?.WaitForThenReset();

            frameData.UniformBufferScratchAllocator.Reset();
            frameData.DescriptorPool.Reset();
        }

        AcquireNextImage();

        var commandBuffer = frameData.CommandBuffer;// _device.GetQueue(VulkanQueueType.Graphics).CreateCommandBuffer();

        commandBuffer.BeginRecording();

        GraphicsContext.BeginRecording(commandBuffer, frameData.UniformBufferScratchAllocator, frameData.DescriptorPool);

        GraphicsContext.TransitionImageLayout(_swapchain.BackbufferToRenderInto.Image, VkImageLayout.Undefined, VkImageLayout.ColorAttachmentOptimal);
    }

    public unsafe void FrameEnd()
    {
        ref var frameData = ref GetCurrentFrameData();

        GraphicsContext.TransitionImageLayout(_swapchain.BackbufferToRenderInto.Image, VkImageLayout.ColorAttachmentOptimal, VkImageLayout.PresentSrcKHR);

        GraphicsContext.EndRecording();

        VkPipelineStageFlags wait_stage = VkPipelineStageFlags.ColorAttachmentOutput;
        VkSemaphore waitSemaphore = frameData.SwapchainAcquireSemaphore;
        VkSemaphore signalSemaphore = frameData.SwapchainReleaseSemaphore;

        VkCommandBuffer commandBuffer = GraphicsContext.CommandBuffer.Handle;

        _device.PresentQueue.Submit(GraphicsContext.CommandBuffer, new VkSubmitInfo
        {
            commandBufferCount = 1,
            pCommandBuffers = &commandBuffer,
            waitSemaphoreCount = 1u,
            pWaitSemaphores = &waitSemaphore,
            pWaitDstStageMask = &wait_stage,
            signalSemaphoreCount = 1u,
            pSignalSemaphores = &signalSemaphore
        }, frameData.SubmitFence);

        //frameData.SubmitFence = GraphicsContext.CommandBuffer.Fence;
        //frameData.SubmitFence.FrameSubmitted = _frameIndex;
        //frameData.FenceSignalCount = frameData.SubmitFence.SignalCount;

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
