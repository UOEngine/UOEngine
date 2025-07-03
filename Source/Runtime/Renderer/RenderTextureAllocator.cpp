#include "Renderer/RenderTextureAllocator.h"

#include <d3d12.h>

#include "Renderer/D3D12Resource.h"
#include "Renderer/RenderDevice.h"

RenderTextureAllocator::RenderTextureAllocator(RenderDevice* InRenderDevice)
{
	mDevice = InRenderDevice;
}

D3D12Resource* RenderTextureAllocator::Allocate(uint32 Width, uint32 Height)
{
	TComPtr<ID3D12Resource> resource;

	D3D12_HEAP_PROPERTIES heap_properties = {};
	heap_properties.Type = D3D12_HEAP_TYPE_DEFAULT;
	heap_properties.CPUPageProperty = D3D12_CPU_PAGE_PROPERTY_UNKNOWN;
	heap_properties.MemoryPoolPreference = D3D12_MEMORY_POOL_UNKNOWN;
	heap_properties.CreationNodeMask = 1;
	heap_properties.VisibleNodeMask = 1;

	D3D12_RESOURCE_DESC desc = {};

	desc.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D;
	desc.Alignment = 0;
	desc.Width = Width;
	desc.Height = Height;
	desc.DepthOrArraySize = 1;
	desc.MipLevels = 1;
	desc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
	desc.Flags = D3D12_RESOURCE_FLAG_NONE;
	desc.Layout = D3D12_TEXTURE_LAYOUT_UNKNOWN;
	desc.SampleDesc.Count = 1;
	desc.SampleDesc.Quality = 0;

	mDevice->GetDevice()->CreateCommittedResource(&heap_properties, D3D12_HEAP_FLAG_NONE, &desc, D3D12_RESOURCE_STATE_COMMON, nullptr, IID_PPV_ARGS(&resource));

	mTextureResources.Add(resource);

	D3D12Resource* new_resource = new D3D12Resource();

	new_resource->SetResource(resource);

	return new_resource;
}

void RenderTextureAllocator::FlushPendingUploads(RenderCommandContext* Context)
{
	// Copy pending resources;

	//for (ID3D12Resource* texture_resource : mPendingTexturesToUpload)
	{
		//Context->
	}


}
