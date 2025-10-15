using Vortice.DXGI;
using UOEngine.Runtime.D3D12.Resources;
using Vortice.Direct3D12;
using SharpGen.Runtime;

namespace UOEngine.Runtime.D3D12;

internal class D3D12Swapchain
{
    public D3D12Texture Backbuffer => _backbufferTextures[_swapChain.CurrentBackBufferIndex];

    private static readonly uint _backbufferCount = 3;

    private readonly D3D12Texture[] _backbufferTextures = new D3D12Texture[_backbufferCount];
    private readonly D3D12Device _device = null!;
    private readonly Format _format = Format.R8G8B8A8_UNorm;

    private IDXGISwapChain3 _swapChain = null!;
    private D3D12CommandQueue _commandQueue = null!;

    private uint _width = 0;
    private uint _height = 0;

    private uint _backbufferIndex = 0;

    public D3D12Swapchain(D3D12Device device)
    {
        _device = device;
    }

    public void Startup(IDXGIFactory2 dxgiFactory2, IntPtr nativeWindowHandle, uint windowWidth, uint windowHeight)
    {
        _width = windowWidth;
        _height = windowHeight;

        for (int i = 0; i < _backbufferCount; i++)
        {
            _backbufferTextures[i] = new D3D12Texture(_device);
        }

        _commandQueue = _device.GetQueue(CommandListType.Direct);
        
        SwapChainDescription1 description = new()
        {
            Width = windowWidth,
            Height = windowHeight,
            Format = _format,
            Stereo = false,
            SampleDescription = SampleDescription.Default,
            BufferUsage = Usage.RenderTargetOutput,
            BufferCount = _backbufferCount,
            SwapEffect = SwapEffect.FlipDiscard,
            Scaling = Scaling.None,
            AlphaMode = AlphaMode.Unspecified
        };

        _swapChain = dxgiFactory2.CreateSwapChainForHwnd(_device.DirectQueue.Handle.As<IUnknown>(), nativeWindowHandle, description).QueryInterface<IDXGISwapChain3>();

        CreateBackbufferTextures();
    }

    public void Resize(uint newWidth, uint newHeight)
    {
        if(_width == newWidth && newHeight == _height)
        {
            return;
        }

        _width = newWidth;
        _height = newHeight;

        _device.GetQueue(CommandListType.Direct).WaitUntilIdle();

        _swapChain.ResizeBuffers(_backbufferCount, _width, _height, _swapChain.Description1.Format, _swapChain.Description1.Flags);

        CreateBackbufferTextures();
    }

    public void Present(D3D12CommandContext commandContext)
    {
        commandContext.TransitionResource(_backbufferTextures[_swapChain.CurrentBackBufferIndex].Resource, ResourceStates.RenderTarget, ResourceStates.Present);
        commandContext.FlushCommands();

        _swapChain.Present(1, 0);
    }

    private void CreateBackbufferTextures()
    {
        for(uint i = 0; i < _backbufferCount; i++)
        {
            ID3D12Resource backbuffer = _swapChain.GetBuffer<ID3D12Resource>(i);

            backbuffer.Name = $"Backbuffer{i}";

            _backbufferTextures[i].InitFromExternalResource(_device, backbuffer, true);
        }
    }
}
