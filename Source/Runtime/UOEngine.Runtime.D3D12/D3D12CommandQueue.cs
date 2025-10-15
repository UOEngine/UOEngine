using Vortice.Direct3D12;

namespace UOEngine.Runtime.D3D12;

internal class D3D12CommandQueue
{
    public readonly ID3D12CommandQueue Handle;

    public ulong SubmissionFenceValue;
    public ID3D12Fence SubmissionFence;

    private readonly CommandListType _type;
    private readonly D3D12Device _device;
    private readonly ID3D12Fence _fence;

    private ulong _submissionFenceValue = 0;

    public D3D12CommandQueue(D3D12Device device, CommandListType type)
    {
        _type = type;
        _device = device;

        CommandQueueDescription description = new()
        {
            Flags = CommandQueueFlags.None,
            Type = _type,
            Priority = (int)CommandQueuePriority.Normal
        };

        Handle = _device.Handle.CreateCommandQueue(description);
        _fence = _device.Handle.CreateFence();
    }

    public void WaitUntilIdle()
    {
        using ID3D12Fence fence = _device.Handle.CreateFence();

        Handle.Signal(fence, 1);

        var spinner = new SpinWait();

        while (fence.CompletedValue < 1)
        {
            spinner.SpinOnce();
        }
    }

    public void ExecuteCommandList(D3D12CommandList commandList)
    {
        Handle.ExecuteCommandList(commandList.Handle);

        _submissionFenceValue++;

        Handle.Signal(_fence, _submissionFenceValue);
    }
}
