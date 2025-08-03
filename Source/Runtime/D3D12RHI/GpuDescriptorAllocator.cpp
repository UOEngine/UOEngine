#include "GpuDescriptorAllocator.h"

#include "Core/Assert.h"
#include "RenderDevice.h"

void GpuDescriptorAllocator::Init(RenderDevice* inRenderDevice, D3D12_DESCRIPTOR_HEAP_TYPE inHeapType, uint32 inNumDescriptors)
{
	GAssert(inHeapType <= D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);

	mType = inHeapType;

	mHandleSize = inRenderDevice->GetDescriptorHandleSize(inHeapType);

	GAssert(mHandleSize > 0);
	mSize = inNumDescriptors;

	D3D12_DESCRIPTOR_HEAP_DESC desc = {};

	desc.Type = inHeapType;
	desc.NumDescriptors = inNumDescriptors;
	desc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;

	mDescriptorHeap = inRenderDevice->CreateDescriptorHeap(&desc);
}

DescriptorTable GpuDescriptorAllocator::Allocate(uint32 inCount)
{
	DescriptorTable table;

	if (mCurrentOffset + inCount <= mSize)
	{
		uint32 handle_offset = mCurrentOffset * mHandleSize;

		table.mGpuHandle.ptr = mDescriptorHeap->GetGPUDescriptorHandleForHeapStart().ptr + handle_offset;
		table.mCpuHandle.ptr = mDescriptorHeap->GetCPUDescriptorHandleForHeapStart().ptr + handle_offset;

		mCurrentOffset += inCount;
	}
	else
	{
		GAssert(false);
	}

	return table;
}
