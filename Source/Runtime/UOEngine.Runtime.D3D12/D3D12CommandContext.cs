using UOEngine.Runtime.D3D12.Resources;
using Vortice.Direct3D12;

namespace UOEngine.Runtime.D3D12;

internal class D3D12CommandContext
{
    public D3D12Texture RenderTarget { get; set; }
     

    private ID3D12GraphicsCommandList1 CommandList { get; set; }

    private readonly D3D12Device _device;
    private readonly CommandListType _type;

    public D3D12CommandContext(D3D12Device device)
    {
        _device = device;
    }

    public void TransitionResource(ID3D12Resource resource, ResourceStates before, ResourceStates after)
    {
        CommandList.ResourceBarrierTransition(resource, before, after);

    }

    public void FlushCommands()
    {
        CommandList.Close();

        _device.GetQueue(_type).ExecuteCommandList();
    }

    public void Draw(uint numInstances)
    {
        CommandList.DrawInstanced(4, numInstances, 0, 0);
    }
}
