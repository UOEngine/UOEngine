using Vortice.Direct3D12;

namespace UOEngine.Runtime.D3D12;

struct D3D12DescriptorHandleCPU
{
    public readonly bool IsValid => (Handle.Ptr != UInt64.MaxValue);
    public CpuDescriptorHandle Handle;
}

internal class D3D12FreeList
{
    public int Size;
    private readonly D3D12DescriptorHandleCPU[] _data;
    private int _inUse = 0;

    public D3D12FreeList(uint capacity)
    {
        _data = new D3D12DescriptorHandleCPU[capacity];
    }

    public void Push(in D3D12DescriptorHandleCPU handle)
    {
        _data[_inUse++] = handle;
    }

    public void Pop(out D3D12DescriptorHandleCPU handle)
    {
        handle = _data[--_inUse];
    }
}
