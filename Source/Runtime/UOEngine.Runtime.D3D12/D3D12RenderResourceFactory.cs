using UOEngine.Runtime.D3D12.Resources;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.D3D12;

internal class D3D12RenderResourceFactory : IRenderResourceFactory
{
    private readonly D3D12Device _device;

    public D3D12RenderResourceFactory(D3D12Device device)
    {
        _device = device;
    }

    public IRenderTexture CreateTexture(int width, int height)
    {
        return new D3D12Texture(_device);
    }
}
