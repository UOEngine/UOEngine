using System.Diagnostics;
using Vortice.Direct3D12;

namespace UOEngine.Runtime.D3D12;

internal class D3D12DescriptorAllocator
{
    private readonly D3D12FreeList _freeHandles;
    private readonly DescriptorHeapType _heapType;
    private readonly uint _numDescriptors;

    private ID3D12DescriptorHeap _heap = null!;
    private uint _lastFreeIndex = 0;
    private uint _handleSize = 0;

    public D3D12DescriptorAllocator(DescriptorHeapType heapType, uint numDescriptors)
    {
        _freeHandles = new D3D12FreeList(numDescriptors);
        _heapType = heapType;
        _numDescriptors = numDescriptors;
    }

    public void Startup(D3D12Device device)
    {
        _handleSize = device.Handle.GetDescriptorHandleIncrementSize(_heapType);

        DescriptorHeapDescription description = new()
        {
            Type = _heapType,
            DescriptorCount = _numDescriptors,
            Flags = 0
        };

        _heap = device.Handle.CreateDescriptorHeap(description);
    }

    public void Allocate(out D3D12DescriptorHandleCPU handle)
    {
        if(_freeHandles.Size > 0)
        {
            _freeHandles.Pop(out handle);
        }
        else if (_lastFreeIndex < _numDescriptors)
        {
            handle = new D3D12DescriptorHandleCPU();

            handle.Handle.Ptr = _heap.GetCPUDescriptorHandleForHeapStart().Ptr + _lastFreeIndex * _handleSize;

            _lastFreeIndex++;
        }
        else
        {
            Debug.Assert(false);

            handle = new D3D12DescriptorHandleCPU();
        }
    }

    public void Free(in D3D12DescriptorHandleCPU handle)
    {
        _freeHandles.Push(handle);
    }
}
