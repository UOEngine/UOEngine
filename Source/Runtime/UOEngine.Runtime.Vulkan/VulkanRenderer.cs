// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
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

    //private VulkanGraphicsContext? _graphicsContext;

    //private VulkanGraphicsContext GraphicsContext => _graphicsContext ?? throw new InvalidOperationException("Graphics context not initialised");

    private VulkanGlobalSamplers _globalSamplers = null!;

    private VulkanContextManager _contextManager = null!;

    private struct PerFrameData
    {
        public VulkanFence SubmitFence;
        public uint FenceSignalCount;
        public VkSemaphore SwapchainAcquireSemaphore;
        public VkSemaphore SwapchainReleaseSemaphore;
        //public VulkanScratchBlockAllocator UniformBufferScratchAllocator;
        //public VulkanDescriptorPool DescriptorPool;
        //public VulkanCommandBuffer CommandBuffer;

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

        VulkanDebug.Init(_device, _instance);

        _resourceFactory.SetDevice(_device);

        _swapchain = new VulkanSwapchain(_instance, _device);

        _swapchain.Create(_window.Handle);

        for(int i = 0; i < 2; i++)
        {
            ref var frameData = ref _perFrameData[i];

            frameData.SubmitFence = new VulkanFence(_device, true);
            frameData.SwapchainAcquireSemaphore = _device.CreateSemaphore();
            frameData.SwapchainReleaseSemaphore = _device.CreateSemaphore();
            //frameData.UniformBufferScratchAllocator = new VulkanScratchBlockAllocator(_device, $"UniformBufferScratchAllocator{i}");
            //frameData.DescriptorPool = new VulkanDescriptorPool(_device);
            //frameData.CommandBuffer = _device.GraphicsQueue.CreateCommandBuffer();
        }

        _globalSamplers = new VulkanGlobalSamplers(_device);
        _contextManager = new VulkanContextManager(_device);
        //_graphicsContext = new VulkanGraphicsContext(_device, _globalSamplers);

    }

    public void Shutdown()
    {
        _instance.Destroy();
    }

    public void FrameBegin()
    {
        _frameIndex++;
        _device.DeferredDeletionQueue.CurrentFrame = _frameIndex;

        //Debug.WriteLine($"VulkanRenderer.FrameBegin: {_frameIndex}");

        ref var frameData = ref GetCurrentFrameData();

        //_device.WaitForGpuIdle();
        // Is the previous frame finished?
        //if(frameData.FenceSignalCount <= frameData.SubmitFence?.SignalCount)
        {
            frameData.SubmitFence?.Refresh();
            frameData.SubmitFence?.WaitForThenReset();

            //frameData.UniformBufferScratchAllocator.Reset();
            //frameData.DescriptorPool.Reset();
        }

        AcquireNextImage();

        //var startFrameContext = _contextManager.AllocateGraphicsContext("EndFrame");

        //startFrameContext.CommandBuffer.EnsureState(_swapchain.BackbufferToRenderInto, RhiRenderTextureUsage.ColourTarget);
        //startFrameContext.EndRecording();

        //var commandBuffer = frameData.CommandBuffer;// _device.GetQueue(VulkanQueueType.Graphics).CreateCommandBuffer();

        //commandBuffer.BeginRecording();

        //GraphicsContext.BeginRecording(commandBuffer, frameData.UniformBufferScratchAllocator, frameData.DescriptorPool);

        //GraphicsContext.TransitionImageLayout(_swapchain.BackbufferToRenderInto.Image, VkImageLayout.Undefined, VkImageLayout.ColorAttachmentOptimal);
    }

    public unsafe void FrameEnd()
    {
        ref var frameData = ref GetCurrentFrameData();

        var endFrameContext = _contextManager.AllocateGraphicsContext("PresentContext");

        endFrameContext.BeginRecording();

        var renderTarget = new RhiRenderTarget();

        renderTarget.Setup(_swapchain.BackbufferToRenderInto);

        // Tell it to wait on the render pipeline.
        //endFrameContext.CommandBuffer.EnsureState(_swapchain.BackbufferToRenderInto, RhiRenderTextureUsage.ColourTarget);
        endFrameContext.BeginRenderPass(new RenderPassInfo
        {
            RenderTarget = renderTarget,
            Name = "Compose"
        });
        endFrameContext.EndRenderPass();
        endFrameContext.CommandBuffer.EnsureState(_swapchain.BackbufferToRenderInto, RhiRenderTextureUsage.Present);
        endFrameContext.EndRecording();

        VkPipelineStageFlags waitStage = VkPipelineStageFlags.ColorAttachmentOutput;
        VkSemaphore waitSemaphore = frameData.SwapchainAcquireSemaphore;
        VkSemaphore signalSemaphore = frameData.SwapchainReleaseSemaphore;

        //VkCommandBuffer commandBuffer = endFrameContext.CommandBuffer.Handle;

        //_device.PresentQueue.Submit(endFrameContext.CommandBuffer, new VkSubmitInfo
        //{
        //    commandBufferCount = 1,
        //    pCommandBuffers = &commandBuffer,
        //    waitSemaphoreCount = 1u,
        //    pWaitSemaphores = &waitSemaphore,
        //    pWaitDstStageMask = &wait_stage,
        //    signalSemaphoreCount = 1u,
        //    pSignalSemaphores = &signalSemaphore
        //}, frameData.SubmitFence);

        SubmitContext(endFrameContext, [waitSemaphore], [waitStage], [signalSemaphore], frameData.SubmitFence);
        //frameData.SubmitFence = GraphicsContext.CommandBuffer.Fence;
        //frameData.SubmitFence.FrameSubmitted = _frameIndex;
        //frameData.FenceSignalCount = frameData.SubmitFence.SignalCount;

        _swapchain.Present(frameData.SwapchainReleaseSemaphore);

        _device.DeferredDeletionQueue.ReleaseResources(_frameIndex);
    }

    public IRenderContext CreateRenderContext(string name) => _contextManager.AllocateGraphicsContext(name);

    public RhiRenderTarget GetViewportRenderTarget()
    {
        var renderTarget = new RhiRenderTarget();

        renderTarget.Setup(_swapchain.BackbufferToRenderInto);

        return renderTarget;
    }

    public void GetInteropContext(out RhiInteropContext interopContext)
    {
        interopContext = new RhiInteropContext
        {
            Instance = _instance.Instance,
            PhysicalDevice = _device.PhysicalDeviceHandle,
            Device = _device.Handle,
            GraphicsQueue = _device.GraphicsQueue.Handle,
            GetProcAddress = (name, instance, device) =>
            {
                if (device != 0)
                {
                    unsafe
                    {
                        return (nint)_instance.Api.vkGetDeviceProcAddr(device, name).Value;
                    }
                    //var p = _instance.Api.GetDeviceProcAddress(new((ulong)device), name);
                    //if (p != 0) return p;
                }

                //if (instance != 0)
                {
                    unsafe
                    {
                        return (nint)vkGetInstanceProcAddr(instance, name).Value;
                    }
                }

                //return vkGetInstanceProcAddr(default, name);
            }
        };
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

    private unsafe void SubmitContext(VulkanGraphicsContext context, ReadOnlySpan<VkSemaphore> waitSemaphores, 
        ReadOnlySpan<VkPipelineStageFlags> waitStages, ReadOnlySpan<VkSemaphore> signalSemaphores, VulkanFence fence)
    {
        VkCommandBuffer cmd = context.CommandBuffer.Handle;

        fixed (VkSemaphore* pWait = waitSemaphores)
        fixed (VkPipelineStageFlags* pStages = waitStages)
        fixed (VkSemaphore* pSignal = signalSemaphores)
        {
            VkSubmitInfo submitInfo = new()
            {
                commandBufferCount = 1,
                pCommandBuffers = &cmd,
                waitSemaphoreCount = (uint)waitSemaphores.Length,
                pWaitSemaphores = pWait,
                pWaitDstStageMask = pStages,
                signalSemaphoreCount = (uint)signalSemaphores.Length,
                pSignalSemaphores = pSignal
            };

            _device.GraphicsQueue.Submit(context.CommandBuffer, submitInfo, fence);
        }

        context.SubmitFence = fence;
        _contextManager.Release(context);
    }

}
