#pragma once

#include "Core/TComPtr.h"

class RenderDevice;
struct ID3D12Resource;

class RenderUploadBuffer
{
public:

	void					Init(RenderDevice* Device, uint32 Size);

	uint8*					Allocate(uint64 Size);

	TComPtr<ID3D12Resource> GetResource() const {return mResource;}

	void					TempUnmap();
private:

	void					Flush();

	uint64					mSize = 0;
	TComPtr<ID3D12Resource> mResource; // The actual buffer.

	uint8*					mMappedPtr;

	uint64					mUsed = 0;
};