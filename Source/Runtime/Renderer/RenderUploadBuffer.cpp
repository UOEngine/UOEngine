#include "Renderer/RenderUploadBuffer.h"

#include "Renderer/RenderDevice.h"

void RenderUploadBuffer::Init(RenderDevice* Device, uint32 Size)
{
	mResource = Device->CreateResource(ERenderHeapType::Upload, Size);

	mSize = Size;

	mResource->Map(0, nullptr, reinterpret_cast<void**>(&mMappedPtr));
}

uint8* RenderUploadBuffer::Allocate(uint64 RequestedSize)
{
	GAssert(RequestedSize <= mSize);

	uint64 bytes_left = mSize - mUsed;
	uint64 offset = mSize - bytes_left;

	if (bytes_left < RequestedSize)
	{
		Flush();
	}

	uint8* allocation = mMappedPtr + offset;

	mUsed += RequestedSize;

	return allocation;
}

void RenderUploadBuffer::TempUnmap()
{
	mResource->Unmap(0, nullptr);
}

void RenderUploadBuffer::Flush()
{
	mResource->Unmap(0, nullptr);
}
