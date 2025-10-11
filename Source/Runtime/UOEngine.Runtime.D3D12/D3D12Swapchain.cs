using Vortice.DXGI;

using UOEngine.Runtime.D3D12.Resources;
using Vortice.Direct3D12;
using SharpGen.Runtime;

namespace UOEngine.Runtime.D3D12;

internal class D3D12Swapchain
{
    private static readonly uint _backbufferCount = 3;

    private readonly D3D12Texture[] _backbufferTextures = new D3D12Texture[_backbufferCount];
    private readonly D3D12Device _device = null!;
    private readonly Format _format = Format.R8G8B8A8_UNorm;

    private IDXGISwapChain1 _swapChain = null!;
    private D3D12CommandQueue _commandQueue = null!;

    public D3D12Swapchain(D3D12Device device)
    {
        _device = device;
    }

    public void Startup(IDXGIFactory2 dxgiFactory2)
    {
        for (int i = 0; i < _backbufferCount; i++)
        {
            _backbufferTextures[i] = new D3D12Texture();
        }

        _commandQueue = _device.GetQueue(CommandListType.Direct);

        nint windowHandle = 0;

        SwapChainDescription1 description = new()
        {
            Format = _format,
            Stereo = false,
            SampleDescription = SampleDescription.Default,
            BufferUsage = Usage.RenderTargetOutput,
            BufferCount = _backbufferCount,
            SwapEffect = SwapEffect.Discard,
            Scaling = Scaling.None,
            AlphaMode = AlphaMode.Unspecified
        };

        _swapChain = dxgiFactory2.CreateSwapChainForHwnd(_commandQueue.As<IUnknown>(), windowHandle, description);

        CreateBackbufferTextures();
    }

    public void Resize()
    {

    }

    public void Present()
    {

    }

    private void CreateBackbufferTextures()
    {
        for(uint i = 0; i < _backbufferCount; i++)
        {
            ID3D12Resource backbuffer = _swapChain.GetBuffer<ID3D12Resource>(i);

            backbuffer.Name = $"Backbuffer{i}";

            _backbufferTextures[i].InitFromExternalResorce(_device, backbuffer);
        }
    }
}
