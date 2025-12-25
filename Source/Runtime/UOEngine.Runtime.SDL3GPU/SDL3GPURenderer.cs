// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using static SDL3.SDL;

using UOEngine.Runtime.Platform;
using UOEngine.Runtime.RHI;
using UOEngine.Runtime.RHI.Resources;
using UOEngine.Runtime.Plugin;

namespace UOEngine.Runtime.SDL3GPU;

[Service(UOEServiceLifetime.Singleton, typeof(IRenderer))]
internal class SDL3GPURenderer : IRenderer
{
    public IRenderSwapChain SwapChain => _swapchain ?? throw new InvalidOperationException("Swapchain is not initialised.");

    private readonly IWindow _window;

    private readonly Sdl3GpuDevice _device;

    private Sdl3GpuGlobalSamplers GlobalSamplers => _globalSamplers ?? throw new InvalidOperationException("Global samplers not initialized.");

    private Sdl3GpuGlobalSamplers? _globalSamplers;

    private IRenderSwapChain? _swapchain;

    public SDL3GPURenderer(IWindow window, Sdl3GpuDevice device)
    {
        _window = window;
        _device = device;
    }

    public void Startup()
    {
        _device.Setup();

        _globalSamplers = new Sdl3GpuGlobalSamplers(_device);

        if (SDL_ClaimWindowForGPUDevice(_device.Handle, _window.Handle) == false)
        {
            throw new Exception("Failed to claim window for GPU device.");
        }

        _swapchain = new SDL3GPUSwapChain(_device, _window.Handle);
    }

    public void Shutdown()
    {
        SDL_DestroyGPUDevice(_device.Handle);
    }

    public IRenderContext CreateRenderContext()
    {
        return new SDL3GPURenderContext(_device, GlobalSamplers);
    }
}
