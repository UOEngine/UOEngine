using Vortice.Direct3D12;

namespace UOEngine.Runtime.D3D12;

internal class D3D12CommandQueue
{
    private readonly CommandListType _type;
    private readonly D3D12Device _device;
    
    private ID3D12CommandQueue _commandQueue = null!;

    public D3D12CommandQueue(D3D12Device device, CommandListType type)
    {
        _type = type;
        _device = device;
    }

    public void WaitUntilIdle()
    {
        using ID3D12Fence fence = _device.Handle.CreateFence();

        _commandQueue.Signal(fence, 1);

        var spinner = new SpinWait();

        while (fence.CompletedValue < 1)
        {
            spinner.SpinOnce();
        }
    }

    public void ExecuteCommandList(D3D12CommandList commandList)
    {

    }
}
