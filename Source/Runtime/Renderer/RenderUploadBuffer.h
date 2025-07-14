#pragma once

#include "Core/Containers/Array.h"
#include "Core/TComPtr.h"

class RenderDevice;
struct ID3D12Resource;

struct RenderUploadBufferAllocation
{
	uint8*	mMappedPtr = nullptr;
	uint64	mOffset = 0;
	uint64	mRange = 0;
};

class RenderUploadBuffer
{
public:

	void							Init(RenderDevice* Device, uint32 Size);

	RenderUploadBufferAllocation*	Allocate(uint64 Size);

	TComPtr<ID3D12Resource>			GetResource() const {return mResource;}

	void							Free(RenderUploadBufferAllocation* inAllocation);
private:

	void							Flush();

	uint64							mSize = 0;
	TComPtr<ID3D12Resource>			mResource; // The actual buffer.

	uint8*							mMappedPtr;

	uint64							mUsed = 0;

	TArray<RenderUploadBufferAllocation*>	mFreeAllocations;
};