#pragma once

#include "Core/Containers/Array.h"
#include "Core/TComPtr.h"
#include "Renderer/DescriptorAllocator.h"

class D3D12Resource;
class D3D12PipelineStateStream;
class DescriptorAllocator;
class GpuDescriptorAllocator;
class RenderCommandAllocator;
class RenderCommandList;
class RenderCommandQueue;
class RenderTextureAllocator;
class RenderUploadBuffer;
enum D3D12_COMMAND_LIST_TYPE;
enum D3D12_DESCRIPTOR_HEAP_TYPE;
struct ID3D12Device4;
struct ID3D12CommandAllocator;
struct ID3D12DescriptorHeap;
struct ID3D12Fence;
struct ID3D12GraphicsCommandList;
struct ID3D12Resource;

enum class ERenderQueueType: uint8
{
	Direct = 0,
	Copy,
	Async,

	Count,
	Invalid
};

enum class ERenderHeapType : uint8
{
	Upload = 0,
	VRAM,
	Count,
	Invalid
};

inline D3D12_HEAP_TYPE RenderHeapTypeToD3D12HeapType(ERenderHeapType HeapType)
{
	D3D12_HEAP_TYPE heap_type;

	switch (HeapType)
	{
		case ERenderHeapType::Upload:
		{
			heap_type = D3D12_HEAP_TYPE_UPLOAD;
		
			break;
		}

		case ERenderHeapType::VRAM:
		{
			heap_type = D3D12_HEAP_TYPE_DEFAULT;

			break;
		}

		default:
			GAssert(false);
	}

	return heap_type;
}

inline D3D12_COMMAND_LIST_TYPE RenderQueueTypeToCommandListType(ERenderQueueType QueueType)
{
	D3D12_COMMAND_LIST_TYPE		Type;

	switch (QueueType)
	{
		case ERenderQueueType::Direct:
		{
			Type = D3D12_COMMAND_LIST_TYPE_DIRECT;

			break;
		}

		case ERenderQueueType::Copy:
		{
			Type = D3D12_COMMAND_LIST_TYPE_COPY;

			break;
		}

		case ERenderQueueType::Async:
		{
			Type = D3D12_COMMAND_LIST_TYPE_COMPUTE;

			break;
		}

		default:
		{
			GAssert(false);
		}
	}

	return Type;
}

class RenderDevice
{
public:

										RenderDevice();

	bool								Initialise(uint32 Flags);

	TComPtr<ID3D12DescriptorHeap>		CreateDescriptorHeap(D3D12_DESCRIPTOR_HEAP_DESC* inDesc);
	DescriptorHandleCPU					CreateRenderTargetView(TComPtr<ID3D12Resource> Resource);
	ID3D12CommandAllocator*				CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE Type);
	ID3D12GraphicsCommandList*			CreateCommandList(ID3D12CommandAllocator* CommandAllocator, D3D12_COMMAND_LIST_TYPE Type);

	void								FreeDescriptor(DescriptorHandleCPU Handle);

	ID3D12Fence*						CreateFence();

	ID3D12Device4*						GetDevice() const													{return mD3D12Device4;}

	uint32								GetDescriptorHandleSize(D3D12_DESCRIPTOR_HEAP_TYPE HeapType) const	{return DescriptorHandleSizes[HeapType]; }

	RenderCommandList*					ObtainCommandList(RenderCommandAllocator* CommandAllocator);

	RenderCommandQueue*					GetQueue(ERenderQueueType QueueType) const							{return Queues[static_cast<int32>(QueueType)]; }

	void								WaitForGpuIdle();

	RenderTextureAllocator*				GetTextureAllocator() const											{return mTextureAllocator;}
	RenderUploadBuffer*					GetUploadBuffer() const												{return mRenderUploadBuffer;}

	D3D12Resource*						CreateResource(ERenderHeapType HeapType, uint64 Size, D3D12_RESOURCE_STATES inInitialState);

	DescriptorAllocator*				GetSrvDescriptorAllocator() const									{return mSrvDescriptorAllocator;}

	GpuDescriptorAllocator*				GetSrvGpuDescriptorAllocator() const								{return GetFrameData().mSrvGpuDescriptorAllocator;}

	void								CreateSampler();
	DescriptorHandleCPU					GetSampler() const													{return mStaticSampler;}

	TComPtr<ID3D12PipelineState>		CreatePipelineState(const D3D12PipelineStateStream* PipelineStream);

	void								BeginFrame();
	void								EndFrame();

	struct FrameData
	{
		GpuDescriptorAllocator* mSrvGpuDescriptorAllocator;
		ID3D12Fence*			mFence;
		uint64					mSubmissionFenceValue = 0;
	};

	uint32								GetFrameDataIndex() const										{ return mFrameCount & 1;}
	const FrameData&					GetFrameData() const											{ return mFrameData[GetFrameDataIndex()]; }

	uint32								GetFrameDataPreviousIndex() const								{return (mFrameCount - 1) & 1; }

private:

	bool								bInitialised;

	ID3D12Device4*						mD3D12Device4;

	int32								NumRenderTargetViewDescriptors = 0;

	uint32*								DescriptorHandleSizes;

	// CPU descriptor heaps
	DescriptorAllocator*				RenderTargetViewDescriptorAllocator;
	DescriptorAllocator*				mSrvDescriptorAllocator;
	DescriptorAllocator*				mSamplerDescriptorAllocator;

	FrameData							mFrameData[2];
	uint32								mFrameCount = 0;

	DescriptorHandleCPU					mStaticSampler;

	TArray<RenderCommandQueue*>			Queues;

	RenderTextureAllocator*				mTextureAllocator;

	RenderUploadBuffer*					mRenderUploadBuffer;

	TArray<TComPtr<ID3D12PipelineState>>mPipelineStates;

	TArray<D3D12Resource*>				mResources;

	void								OnDeviceRemoved();

};

