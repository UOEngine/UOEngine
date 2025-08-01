#include "Renderer/DescriptorAllocator.h"

#include "Renderer/RenderDevice.h"

DescriptorAllocator::DescriptorAllocator()
{
	Type = D3D12_DESCRIPTOR_HEAP_TYPE_NUM_TYPES;
	Num = 0;
	LastFreeIndex = 0;
	HandleSize = 0;
}

void DescriptorAllocator::Init(RenderDevice* Device, D3D12_DESCRIPTOR_HEAP_TYPE HeapType, uint32 NumDescriptors)
{
	HandleSize = Device->GetDescriptorHandleSize(HeapType);

	Type = HeapType;
	Num = NumDescriptors;

	D3D12_DESCRIPTOR_HEAP_DESC heap_desc = {};

	heap_desc.Type = HeapType;
	heap_desc.NumDescriptors = NumDescriptors;

	DescriptorHeap = Device->CreateDescriptorHeap(&heap_desc);

	FreeHandles.Init(NumDescriptors);
}

DescriptorHandleCPU DescriptorAllocator::Allocate()
{
	DescriptorHandleCPU Handle = {DescriptorHandleCPU::Invalid};

	if (FreeHandles.Size > 0)
	{
		Handle = FreeHandles.Pop();
	}
	else if(LastFreeIndex < Num)
	{
		Handle.ptr = DescriptorHeap->GetCPUDescriptorHandleForHeapStart().ptr + LastFreeIndex * HandleSize;
		
		LastFreeIndex++;
	}
	else
	{
		GAssert(false);
	}

	return Handle;
}

void DescriptorAllocator::Free(DescriptorHandleCPU Handle)
{
	GAssert(Handle.IsValid());

	FreeHandles.Push(Handle);
}
