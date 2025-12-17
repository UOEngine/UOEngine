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

    private SDL3GPUTexture _backbufferTexture;

    RhiRenderTarget _backbufferRenderTarget = new();

    public TextureFormat BackbufferFormat => _format.ToRhiFormat();

    public SDL3GPUSwapChain(Sdl3GpuDevice device, IntPtr windowHandle)
    {
        _device = device;
        _windowHandle = windowHandle;

        _format = SDL_GetGPUSwapchainTextureFormat(_device.Handle, windowHandle);
    }

    public RhiRenderTarget? Acquire(IRenderContext context)
    {
        SDL3GPURenderContext sdl3GpuContext = (context as SDL3GPURenderContext)!;

        if(SDL_WaitAndAcquireGPUSwapchainTexture(sdl3GpuContext.RecordedCommands, _windowHandle, out _backbufferToRenderInto, out uint backbufferWidth, out uint backbufferHeight) == false)
        {
            return null;
        }

        if((backbufferWidth != _backbufferWidth) || (backbufferHeight != _backbufferHeight))
        {
            //_device.WaitForGpuIdle();

            //_backbufferTexture?.Dispose();

            _backbufferWidth = backbufferWidth;
            _backbufferHeight = backbufferHeight;

            _backbufferTexture = new SDL3GPUTexture(_device, new SDL3GPUTextureDescription
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


            _backbufferRenderTarget.Setup(_backbufferTexture);
        }

        _backbufferTexture.InitFromExistingResource(_backbufferToRenderInto);

        return _backbufferRenderTarget;
    }
}
