using UOEngine.Runtime.RHI;
using Vortice.Direct3D12;
using Vortice.DXGI;

namespace UOEngine.Runtime.D3D12.Resources;

internal class D3D12Texture: IRenderTexture
{
    public ID3D12Resource Resource { get; private set; } = null!;
    public ulong Width { get; private set; }
    public uint Height { get; private set; }

    public Format Format { get; private set; }

    private readonly D3D12Device _device;

    private D3D12DescriptorHandleCPU _descriptor;

    public D3D12Texture(D3D12Device device)
    {
        _device = device;
    }

    public void InitFromExternalResource(D3D12Device device, ID3D12Resource resource, bool isRenderTarget)
    {
        Resource = resource;

        Width = resource.Description.Width;
        Height = resource.Description.Height;
        Format = resource.Description.Format;

        if (isRenderTarget)
        {
            _device.CreateRenderTargetView(Resource, out _descriptor);
        }
    }

    public void SetDataPointer(UIntPtr pointer, int size)
    {
        throw new NotImplementedException();
    }
}
