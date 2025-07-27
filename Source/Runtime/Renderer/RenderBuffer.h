#pragma once

#include "Core/Containers/Array.h"
#include "Core/Types.h"
#include "Renderer/DescriptorAllocator.h"

class D3D12Resource;
class RenderDevice;
enum DXGI_FORMAT;
struct ID3D12Resource;
struct RenderUploadBufferAllocation;

class RenderBuffer
{
public:

									RenderBuffer();

	void							Init(RenderDevice* inDevice, uint32 inNumElements, uint32 inStride, DXGI_FORMAT inFormat);
	void							Release();

	DescriptorHandleCPU				GetSrv() const { return mSrvDescriptor; }

	TSpan<uint8>					Lock();
	void							Unlock();

private:

	D3D12Resource*					mGpuResource;
	void*							MappedData;

	uint32							mStride;
	uint32							mLength;

	DescriptorHandleCPU				mSrvDescriptor;
	RenderUploadBufferAllocation*	mUploadAllocation;

	RenderDevice*					mDevice;

	bool							mbLocked;
};