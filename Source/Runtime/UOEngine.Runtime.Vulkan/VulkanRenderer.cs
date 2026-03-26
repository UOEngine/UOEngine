// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;

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
        public VkSemaphore SwapchainAcquireSemaphore;
        public VkSemaphore SwapchainReleaseSemaphore;
        public List<VulkanGraphicsContext> Contexts;

    }

    private PerFrameData[] _perFrameData = [];

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

    public void Startup()
    {
        VkResult result = vkInitialize();

        result.CheckResult();

        _instance.Create("UOEngineApp", _enableDebug);

        _device = new VulkanDevice(_instance.GetSuitableDevice());

        VulkanDebug.Init(_device, _instance);

        _device.InitGpu(_instance.Api);

        _resourceFactory.SetDevice(_device);

        _swapchain = new VulkanSwapchain(_instance, _device);

        _swapchain.Create(_window.Handle);

        _perFrameData = new PerFrameData[_swapchain.BackBufferCount];

        for (int i = 0; i < _swapchain.BackBufferCount; i++)
        {
            _perFrameData[i] = new PerFrameData
            {
                SubmitFence = new VulkanFence(_device, true, $"FrameFence{i}"),
                SwapchainAcquireSemaphore = _device.CreateSemaphore(),
                SwapchainReleaseSemaphore = _device.CreateSemaphore(),
                Contexts = []
            };
        }

        _globalSamplers = new VulkanGlobalSamplers(_device);
        _contextManager = new VulkanContextManager(this, _device, _globalSamplers);

    }

    public void Shutdown()
    {
        _instance.Destroy();
    }

    public void FrameBegin()
    {
        _frameIndex++;

        _device.DeferredDeletionQueue.CurrentFrame = _frameIndex;

        ref var frameData = ref GetCurrentFrameData();

        //Debug.WriteLine($"-- Frame {_frameIndex}, Wait for {frameData.SubmitFence.Name} --");

        frameData.SubmitFence.WaitForThenReset();

        foreach(var context in frameData.Contexts)
        {
            _contextManager.Release(context);
        }

        frameData.Contexts.Clear();

        AcquireNextImage();

        _contextManager.OnFrameBegin(_swapchain.BackbufferToRenderInto);

        var acquireContext = _contextManager.AllocateGraphicsContext("AcquireContext");

        acquireContext.CommandBuffer.EnsureState(_swapchain.BackbufferToRenderInto, RhiRenderTextureUsage.ColourTarget);
        acquireContext.WaitForSemaphores = [frameData.SwapchainAcquireSemaphore];
        acquireContext.WaitStages = [VkPipelineStageFlags.ColorAttachmentOutput];
    }

    public unsafe void FrameEnd()
    {
        ref var frameData = ref GetCurrentFrameData();

        var endFrameContext = _contextManager.AllocateGraphicsContext("PresentContext");

        endFrameContext.CommandBuffer.EnsureState(_swapchain.BackbufferToRenderInto, RhiRenderTextureUsage.Present);
        endFrameContext.SignalSemaphores = [frameData.SwapchainReleaseSemaphore];

        int submitCount = _contextManager._inUseGraphicsContexts.Count;

        Span<VkCommandBuffer> commandBuffers = stackalloc VkCommandBuffer[submitCount];

        Span<VkSemaphore> waitSemaphores = stackalloc VkSemaphore[submitCount];
        Span<VkPipelineStageFlags> waitStages = stackalloc VkPipelineStageFlags[submitCount];
        Span<VkSemaphore> signalSemaphores = stackalloc VkSemaphore[submitCount];

        int waitOffset = 0;
        int signalOffset = 0;

        for (int i = 0; i < submitCount; i++)
        {
            var context = _contextManager._inUseGraphicsContexts[i];

            context.EndRecording();

            commandBuffers[i] = context.CommandBuffer.Handle;

            int waitCount = context.WaitForSemaphores.Length;
            int signalCount = context.SignalSemaphores.Length;

            context.WaitForSemaphores.CopyTo(waitSemaphores.Slice(waitOffset, waitCount)); 
            context.WaitStages.CopyTo(waitStages.Slice(waitOffset, waitCount));
            context.SignalSemaphores.CopyTo(signalSemaphores.Slice(signalOffset, signalCount));

            waitOffset += waitCount;
            signalOffset += signalCount;
        }

        Span<VkSubmitInfo> submitInfos = stackalloc VkSubmitInfo[submitCount];

        fixed (VkSubmitInfo* pSubmitInfos = submitInfos)
        fixed (VkCommandBuffer* pCommandBuffers = commandBuffers)
        fixed (VkSemaphore* pWaitSemaphores = waitSemaphores)
        fixed (VkPipelineStageFlags* pWaitStages = waitStages)
        fixed (VkSemaphore* pSignalSemaphores = signalSemaphores)
        {
            waitOffset = 0;
            signalOffset = 0;

            for (int i = 0; i < submitCount; i++)
            {
                var context = _contextManager._inUseGraphicsContexts[i];

                int waitCount = context.WaitForSemaphores.Length;
                int signalCount = context.SignalSemaphores.Length;

                pSubmitInfos[i] = new VkSubmitInfo
                {
                    commandBufferCount = 1,
                    pCommandBuffers = pCommandBuffers + i,
                    waitSemaphoreCount = (uint)waitCount,
                    pWaitSemaphores = waitCount > 0 ? pWaitSemaphores + waitOffset : null,
                    pWaitDstStageMask = waitCount > 0 ? pWaitStages + waitOffset : null,
                    signalSemaphoreCount = (uint)signalCount,
                    pSignalSemaphores = signalCount > 0 ? pSignalSemaphores + signalOffset : null
                };

                waitOffset += waitCount;
                signalOffset += signalCount;
            }

            frameData.SubmitFence.FrameSubmitted = _frameIndex;

            _device.GraphicsQueue.Submit(submitInfos, frameData.SubmitFence);
        }

        //Debug.WriteLine($"I should be waiting for {frameData.SubmitFence.Name} on frame {_frameIndex + VulkanSwapchain.NumImages}");

        _swapchain.Present(frameData.SwapchainReleaseSemaphore);

        _device.DeferredDeletionQueue.ReleaseResources(_frameIndex);

        foreach(var context in _contextManager._inUseGraphicsContexts)
        {
            frameData.Contexts.Add(context);
        }

        _contextManager._inUseGraphicsContexts.Clear();
    }

    public IRenderContext CreateRenderContext(string name) => _contextManager.AllocateGraphicsContext(name);

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

    public IRenderTexture GetBackbufferTexture() => _swapchain.BackbufferToRenderInto;

    internal void SubmitImmediate(VulkanGraphicsContext context)
    {
        ref var frameData = ref GetCurrentFrameData();

        context.EndRecording();

        _device.GraphicsQueue.Submit(context.CommandBuffer, null);

        frameData.Contexts.Add(context);

        _contextManager._inUseGraphicsContexts.Remove(context);
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
