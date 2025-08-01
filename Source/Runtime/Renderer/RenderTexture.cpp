#include "Renderer/RenderTexture.h"

#include "Core/Alignment.h"
#include "Renderer/D3D12Resource.h"
#include "Renderer/RenderContext.h"
#include "Renderer/RenderCommandQueue.h"
#include "Renderer/RenderDevice.h"
#include "Renderer/RenderTextureAllocator.h"
#include "Renderer/RenderUploadBuffer.h"

RenderTexture::RenderTexture()
{
	mResource = nullptr;
	mUploadAllocation = nullptr;
}

void RenderTexture::Init(const TextureDescription& InDescription)
{
	GAssert(InDescription.Width > 0);
	GAssert(InDescription.Height > 0);

	Description = InDescription;

	mDevice = Description.Device;

	if (Description.Resource == nullptr)
	{
		mResource = mDevice->GetTextureAllocator()->Allocate(Description.Width, Description.Height);
	}
	else
	{
		mResource = Description.Resource;
	}

	switch (Description.Type)
	{
		case RenderTextureType::RenderTarget:
		{
			RenderTargetViewDescriptor = mDevice->CreateRenderTargetView(mResource->Get());

			break;
		}

		case RenderTextureType::ReadOnly:
		{
			break;
		}

		default:
		{
			GAssert(false);
		}
	}

	D3D12_SHADER_RESOURCE_VIEW_DESC srv_desc = {};

	srv_desc.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
	srv_desc.Format = mResource->GetDesc().Format;
	srv_desc.ViewDimension = D3D12_SRV_DIMENSION_TEXTURE2D;
	srv_desc.Texture2D.MipLevels = mResource->GetDesc().MipLevels;

	mSrv = mDevice->GetSrvDescriptorAllocator()->Allocate();

	mDevice->GetDevice()->CreateShaderResourceView(mResource->Get(), &srv_desc, mSrv);

	if (InDescription.mInitialData.Num() > 0)
	{
		InitialiseTextureData(InDescription.mInitialData);
	}
}

void RenderTexture::InitFromExternalResource(RenderDevice* InDevice, TComPtr<ID3D12Resource> Resource, bool bRenderTarget)
{
	mDevice = InDevice;
	mResource = new D3D12Resource();

	D3D12_RESOURCE_DESC resource_desc = Resource->GetDesc();

	mResource->SetResource(Resource);

	if (bRenderTarget)
	{
		RenderTargetViewDescriptor = mDevice->CreateRenderTargetView(mResource->Get());
		Description.Type = RenderTextureType::RenderTarget;
	}

	Description.Width = resource_desc.Width;
	Description.Height = resource_desc.Height;
	Description.Format = resource_desc.Format;
}

void RenderTexture::Release()
{
	mResource->Release();

	switch (Description.Type)
	{
		case RenderTextureType::RenderTarget:
		{
			mDevice->FreeDescriptor(RenderTargetViewDescriptor);

			break;
		}

		default:
		{
			GAssert(false);
		}
	}

	RenderTargetViewDescriptor = DescriptorHandleCPU();
}

void RenderTexture::SetName(const String& NewName)
{
	mName = NewName;

	mResource->SetName("TestTexture");
}

TSpan<uint8> RenderTexture::Lock()
{
	GAssert(mbLocked == false);

	mbLocked = true;

	uint64 pitch = Description.Width * 4;
	uint64 aligned_pitch = Alignment::Align(pitch, D3D12_TEXTURE_DATA_PITCH_ALIGNMENT);
	uint64 size_required = Description.Height * aligned_pitch;

	mUploadAllocation = mDevice->GetUploadBuffer()->Allocate(size_required);

	return TSpan<uint8>(mUploadAllocation->mMappedPtr, size_required);
}

void RenderTexture::Unlock()
{
	GAssert(mbLocked);

	RenderCommandQueue*					copy_queue = mDevice->GetQueue(ERenderQueueType::Copy);
	RenderCommandList*					command_list = copy_queue->CreateCommandList();
	D3D12_PLACED_SUBRESOURCE_FOOTPRINT	footprint;
	D3D12_RESOURCE_DESC					resource_desc = mResource->GetDesc();

	mDevice->GetDevice()->GetCopyableFootprints(&resource_desc, 0, 1, 0, &footprint, nullptr, nullptr, nullptr);

	D3D12_TEXTURE_COPY_LOCATION source_copy_location = {};

	// Offset from upload buffer source.
	footprint.Offset += mUploadAllocation->mOffset;

	source_copy_location.pResource = mDevice->GetUploadBuffer()->GetResource()->Get();
	source_copy_location.Type = D3D12_TEXTURE_COPY_TYPE_PLACED_FOOTPRINT;
	source_copy_location.PlacedFootprint = footprint;

	D3D12_TEXTURE_COPY_LOCATION destination_copy_location = {};

	destination_copy_location.pResource = mResource->Get();
	destination_copy_location.Type = D3D12_TEXTURE_COPY_TYPE_SUBRESOURCE_INDEX;
	destination_copy_location.SubresourceIndex = 0;

	command_list->GetGraphicsCommandList()->CopyTextureRegion(&destination_copy_location, 0, 0, 0, &source_copy_location, nullptr);
	command_list->Close();

	copy_queue->ExecuteCommandList(command_list);
	copy_queue->WaitUntilIdle();

	mDevice->GetUploadBuffer()->Free(mUploadAllocation);

	mUploadAllocation = nullptr;

	mbLocked = false;
}

void RenderTexture::InitialiseTextureData(const TSpan<uint8>& inData)
{
	TSpan<uint8> destination = Lock();

	uint32 pitch = Description.Width * 4;
	uint64 aligned_pitch = Alignment::Align(pitch, D3D12_TEXTURE_DATA_PITCH_ALIGNMENT);

	if (pitch == aligned_pitch)
	{
		Memory::MemCopy(destination.GetData(), destination.Num(), inData.GetData(), inData.Num());  
	}
	else
	{
		for(uint32 row = 0; row < Description.Height; row++)
		{
			Memory::MemCopy(destination.GetData() + aligned_pitch * row, destination.Num(), inData.GetData() + pitch * row, pitch);
		}
	}

	Unlock();
}
