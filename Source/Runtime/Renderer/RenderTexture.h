#pragma once

#include <dxgiformat.h>

#include "Core/Containers/String.h"
#include "Core/Containers/Span.h"
#include "Core/TComPtr.h"
#include "Renderer/DescriptorAllocator.h"

class D3D12Resource;
class RenderDevice;
struct RenderUploadBufferAllocation;

enum class RenderTextureType
{
	Invalid,
	RenderTarget,
	ReadOnly
};

struct RenderTextureClearValue
{
	float Colour[4] = {};
};

class RenderTexture
{
public:

								RenderTexture();

	struct TextureDescription
	{
		RenderDevice*			Device = nullptr;
		uint32					Width = 0;
		uint32					Height = 0;
		DXGI_FORMAT				Format = DXGI_FORMAT_UNKNOWN;
		RenderTextureType		Type = RenderTextureType::Invalid;
		D3D12Resource*			Resource = nullptr;
		RenderTextureClearValue ClearValue = {0, 0, 0, 0};
		String					Name;
		TSpan<uint8>			mInitialData;
	};

	void						Init(const TextureDescription& Parameters);

	//	For texture resource created externally e.g. swapchain.
	void						InitFromExternalResource(RenderDevice* InDevice, TComPtr<ID3D12Resource> Resource, bool bRenderTarget);

	void						Release();

	const TextureDescription&	GetDescription() const					{return Description;}

	void						SetName(const String& NewName);
	String						GetName() const							{return mName;}

	DescriptorHandleCPU			GetRenderTargetViewDescriptor() const	{return RenderTargetViewDescriptor;}

	DescriptorHandleCPU			GetSrv() const							{return mSrv;}

	D3D12Resource*				GetResource() const						{return mResource;}

	TSpan<uint8>				Lock();
	void						Unlock();

	void						InitialiseTextureData(const TSpan<uint8>& inData);

private:

	TextureDescription			Description;

	DescriptorHandleCPU			RenderTargetViewDescriptor;

	TArray<uint8>				mDataToUpload;

	RenderUploadBufferAllocation* mUploadAllocation;

	bool						mbLocked = false;

	RenderDevice*				mDevice = nullptr;

	D3D12Resource*				mResource; // Resource on the GPU.
	DescriptorHandleCPU			mSrv = { DescriptorHandleCPU::Invalid };

	String						mName;
};
