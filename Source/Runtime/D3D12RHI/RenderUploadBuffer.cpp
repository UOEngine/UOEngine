#include "RenderUploadBuffer.h"

#include <d3d12.h>

#include "D3D12Resource.h"
#include "RenderDevice.h"

void RenderUploadBuffer::Init(RenderDevice* Device, uint32 Size)
{
	mResource = Device->CreateResource(ERenderHeapType::Upload, Size, D3D12_RESOURCE_STATE_GENERIC_READ);

	mSize = Size;

	mResource->Get()->Map(0, nullptr, reinterpret_cast<void**>(&mMappedPtr));

}

RenderUploadBufferAllocation* RenderUploadBuffer::Allocate(uint64 inRequestedSize)
{
	GAssert(inRequestedSize <= mSize);

	uint64 bytes_left = mSize - mUsed;
	uint64 offset = mSize - bytes_left;

	if (bytes_left < inRequestedSize)
	{
		GAssert(false);

		Flush();
	}

	mUsed += inRequestedSize;

	D3D12_RANGE range;

	range.Begin = offset;
	range.End = offset + inRequestedSize;

	RenderUploadBufferAllocation* allocation = nullptr;

	if (mFreeAllocations.Num() == 0)
	{
		allocation = new RenderUploadBufferAllocation();
	}
	else
	{
		allocation = mFreeAllocations.Last();

		mFreeAllocations.PopBack();
	}

	allocation->mMappedPtr = mMappedPtr + offset;
	allocation->mOffset = offset;
	allocation->mRange = inRequestedSize;

	return allocation;
}

void RenderUploadBuffer::Free(RenderUploadBufferAllocation* inAllocation)
{
	inAllocation->mMappedPtr = nullptr;
	inAllocation->mOffset = 0;
	inAllocation->mRange = 0;

	mFreeAllocations.Add(inAllocation);
}

void RenderUploadBuffer::Flush()
{
	GAssert(false);
}
