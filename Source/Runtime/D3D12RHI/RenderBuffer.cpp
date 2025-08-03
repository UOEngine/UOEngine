#include "RenderBuffer.h"

#include "Core/Alignment.h"

#include "D3D12Resource.h"
#include "RenderCommandList.h"
#include "RenderCommandQueue.h"
#include "RenderUploadBuffer.h"
#include "RenderDevice.h"

RenderBuffer::RenderBuffer()
{
	mGpuResource = nullptr;
	MappedData = nullptr;

	mStride = 0;
	mLength = 0;

	mDevice = nullptr;

	mbLocked = false;
}

void RenderBuffer::Init(RenderDevice* inDevice, uint32 inNumElements, uint32 inStride, DXGI_FORMAT inFormat)
{
	mLength = inNumElements * inStride;
	mStride = inStride;
	mDevice = inDevice;

	mGpuResource = inDevice->CreateResource(ERenderHeapType::VRAM, mLength, D3D12_RESOURCE_STATE_COMMON);

	mSrvDescriptor = inDevice->GetSrvDescriptorAllocator()->Allocate();

	// Tell GPU how to interpret it.
	
	D3D12_SHADER_RESOURCE_VIEW_DESC srv_desc = {};

	srv_desc.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
	srv_desc.Format = inFormat;
	srv_desc.ViewDimension = D3D12_SRV_DIMENSION_BUFFER;
	srv_desc.Buffer.FirstElement = 0;
	srv_desc.Buffer.NumElements = inNumElements;
	srv_desc.Buffer.StructureByteStride = (inFormat == DXGI_FORMAT_UNKNOWN) ? mStride : 0;
	srv_desc.Buffer.Flags = D3D12_BUFFER_SRV_FLAG_NONE;

	inDevice->GetDevice()->CreateShaderResourceView(mGpuResource->Get(), &srv_desc, mSrvDescriptor);
}

void RenderBuffer::Release()
{

}

TSpan<uint8> RenderBuffer::Lock()
{
	GAssert(mbLocked == false);

	mbLocked = true;

	uint64 size_required = Alignment::Align(mLength, mStride);

	mUploadAllocation = mDevice->GetUploadBuffer()->Allocate(size_required);

	return TSpan<uint8>(mUploadAllocation->mMappedPtr, size_required);
}

void RenderBuffer::Unlock()
{
	GAssert(mbLocked);

	RenderCommandQueue* copy_queue = mDevice->GetQueue(ERenderQueueType::Copy);
	RenderCommandList* command_list = copy_queue->CreateCommandList();

	command_list->GetGraphicsCommandList()->CopyBufferRegion(mGpuResource->Get(), 0, mDevice->GetUploadBuffer()->GetResource()->Get(), mUploadAllocation->mOffset, mLength);
	command_list->Close();

	copy_queue->ExecuteCommandList(command_list);
	copy_queue->WaitUntilIdle();

	mDevice->GetUploadBuffer()->Free(mUploadAllocation);

	mUploadAllocation = nullptr;

	mbLocked = false;
}
