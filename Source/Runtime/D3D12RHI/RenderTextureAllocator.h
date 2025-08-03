#pragma once

#include "Core/Containers/Array.h"
#include "Core/TComPtr.h"

class D3D12Resource;
struct ID3D12Resource;
class RenderCommandContext;
class RenderDevice;

//struct RenderResourceAllocation
//{
//	void*						mCPUAddress;
//	D3D12_GPU_VIRTUAL_ADDRESS	mGPUAddress;
//};

class RenderTextureAllocator
{
public:

										RenderTextureAllocator(RenderDevice* InRenderDevice);

	D3D12Resource*						Allocate(uint32 Width, uint32 Height);

	void								FlushPendingUploads(RenderCommandContext* Context);

private:

	RenderDevice*						mDevice;

	TArray<TComPtr<ID3D12Resource>>		mTextureResources;
	TArray<ID3D12Resource*>				mPendingTexturesToUpload;
};