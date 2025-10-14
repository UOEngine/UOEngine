using Vortice.Direct3D12;

namespace UOEngine.Runtime.D3D12.Resources;

internal class D3D12Texture
{
    public ID3D12Resource Resource { get; private set; } = null!;

    public void InitFromExternalResorce(D3D12Device device, ID3D12Resource resource)
    {
        Resource = resource;
    }
}
