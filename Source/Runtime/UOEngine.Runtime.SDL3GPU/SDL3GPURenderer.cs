using static SDL3.SDL;

using UOEngine.Runtime.Platform;
using UOEngine.Runtime.RHI;
using UOEngine.Runtime.RHI.Resources;

namespace UOEngine.Runtime.SDL3GPU;

internal class SDL3GPURenderer : IRenderer
{
    public IRenderSwapChain SwapChain { get; private set; }

    private readonly IWindow _window;

    private readonly Sdl3GpuDevice _device;

    private Sdl3GpuGlobalSamplers _globalSamplers;


    public SDL3GPURenderer(IWindow window, IRenderDevice device)
    {
        _window = window;
        _device = (Sdl3GpuDevice)device;
    }

    public void Startup()
    {
        _device.Setup();

       if(SDL_ClaimWindowForGPUDevice(_device.Handle, _window.Handle) == false)
       {
            throw new Exception("Failed to claim window for GPU device.");
       }

        SwapChain = new SDL3GPUSwapChain(_device, _window.Handle);
        _globalSamplers = new Sdl3GpuGlobalSamplers(_device);
    }

    public void Shutdown()
    {
        SDL_DestroyGPUDevice(_device.Handle);
    }

    public IRenderContext CreateRenderContext()
    {
        return new SDL3GPURenderContext(_device, _globalSamplers);
    }
}
