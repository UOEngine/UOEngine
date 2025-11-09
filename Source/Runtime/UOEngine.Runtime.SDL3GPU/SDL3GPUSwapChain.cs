using static SDL3.SDL;
using UOEngine.Runtime.RHI;
using UOEngine.Runtime.RHI.Resources;
using UOEngine.Runtime.SDL3GPU.Resources;

namespace UOEngine.Runtime.SDL3GPU;

internal class SDL3GPUSwapChain: IRenderSwapChain
{
    private IntPtr _backbufferToRenderInto;
    private uint _backbufferWidth;
    private uint _backbufferHeight;

    private readonly IntPtr _windowHandle;
    private readonly Sdl3GpuDevice _device;

    private readonly SDL_GPUTextureFormat _format;

    RenderTarget _backbufferRenderTarget = new();

    public TextureFormat BackbufferFormat => _format.ToRhiFormat();

    public SDL3GPUSwapChain(Sdl3GpuDevice device, IntPtr windowHandle)
    {
        _device = device;
        _windowHandle = windowHandle;

        _format = SDL_GetGPUSwapchainTextureFormat(_device.Handle, windowHandle);
    }

    public RenderTarget? Acquire(IRenderContext context)
    {
        SDL3GPURenderContext sdl3GpuContext = (context as SDL3GPURenderContext)!;

        if(SDL_WaitAndAcquireGPUSwapchainTexture(sdl3GpuContext.RecordedCommands, _windowHandle, out _backbufferToRenderInto, out _backbufferWidth, out _backbufferHeight) == false)
        {
            return null;
        }

       var backbufferTexture = new SDL3GPUTexture(_device, new SDL3GPUTextureDescription
       {
           CreateInfo = new SDL_GPUTextureCreateInfo
           {
               width = _backbufferWidth,
               height = _backbufferHeight,
               usage = SDL_GPUTextureUsageFlags.SDL_GPU_TEXTUREUSAGE_COLOR_TARGET,
               format = _format
           },
           Name = "Backbuffer",
        });

        backbufferTexture.InitFromExistingResource(_backbufferToRenderInto);

        _backbufferRenderTarget.Setup(backbufferTexture);

        return _backbufferRenderTarget;
    }
}
