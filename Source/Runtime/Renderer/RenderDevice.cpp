#include "Renderer/RenderDevice.h"

#include <d3d12.h>
#include <dxgi1_6.h>

#include "Core/Assert.h"
#include "Renderer/D3D12Resource.h"
#include "Renderer/D3D12PipelineStream.h"
#include "Renderer/DescriptorAllocator.h"
#include "Renderer/GpuDescriptorAllocator.h"
#include "Renderer/RenderCommandAllocator.h"
#include "Renderer/RenderCommandQueue.h"
#include "Renderer/RenderTextureAllocator.h"
#include "Renderer/RenderUploadBuffer.h"

RenderDevice::RenderDevice()
{
	bInitialised = false;
	DescriptorHandleSizes = nullptr;
	mD3D12Device4 = nullptr;

	mTextureAllocator = nullptr;
}


bool RenderDevice::Initialise(uint32 DxgiFactoryFlags)
{
	TComPtr<IDXGIFactory4> DxgiFactory;

	if (SUCCEEDED(CreateDXGIFactory2(DxgiFactoryFlags, IID_PPV_ARGS(&DxgiFactory))) == false)
	{
		return false;
	}

	TComPtr<IDXGIAdapter1>	DxgiAdapter1;
	SIZE_T					MaxSize = 0;

	for (int32 i = 0; DxgiFactory->EnumAdapters1(i, &DxgiAdapter1) != DXGI_ERROR_NOT_FOUND; i++)
	{
		DXGI_ADAPTER_DESC1 DxgiAdapterDesc1;

		DxgiAdapter1->GetDesc1(&DxgiAdapterDesc1);

		if (DxgiAdapterDesc1.Flags & DXGI_ADAPTER_FLAG_SOFTWARE)
		{
			continue;
		}

		if (SUCCEEDED(D3D12CreateDevice(DxgiAdapter1, D3D_FEATURE_LEVEL_11_0, IID_PPV_ARGS(&mD3D12Device4))) == false)
		{
			continue;
		}

		// By default, search for the adapter with the most memory because that's usually the dGPU.
		if (DxgiAdapterDesc1.DedicatedVideoMemory < MaxSize)
		{
			continue;
		}

		MaxSize = DxgiAdapterDesc1.DedicatedVideoMemory;

		break;
	}

	if (DxgiFactoryFlags & DXGI_CREATE_FACTORY_DEBUG)
	{
		TComPtr<ID3D12InfoQueue1> info_queue;

		if (SUCCEEDED(mD3D12Device4->QueryInterface(__uuidof(ID3D12InfoQueue), (void**)&info_queue)) == false)
		{
			GAssert(false);
		}

		info_queue->SetBreakOnSeverity(D3D12_MESSAGE_SEVERITY_CORRUPTION, TRUE);
		info_queue->SetBreakOnSeverity(D3D12_MESSAGE_SEVERITY_ERROR, TRUE);
		info_queue->SetBreakOnSeverity(D3D12_MESSAGE_SEVERITY_WARNING, TRUE);
	}

	DescriptorHandleSizes = new uint32[D3D12_DESCRIPTOR_HEAP_TYPE_NUM_TYPES];

	for (int32 i = 0; i < D3D12_DESCRIPTOR_HEAP_TYPE_NUM_TYPES; i++)
	{
		DescriptorHandleSizes[i] = mD3D12Device4->GetDescriptorHandleIncrementSize(static_cast<D3D12_DESCRIPTOR_HEAP_TYPE>(i));
	}

	RenderTargetViewDescriptorAllocator = new DescriptorAllocator();
	RenderTargetViewDescriptorAllocator->Init(this, D3D12_DESCRIPTOR_HEAP_TYPE_RTV, 128);

	mSrvDescriptorAllocator = new DescriptorAllocator();
	mSrvDescriptorAllocator->Init(this, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV, 128);

	// Create simple sampler.
	mSamplerDescriptorAllocator = new DescriptorAllocator();
	mSamplerDescriptorAllocator->Init(this, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER, 8);

	mStaticSampler = mSrvDescriptorAllocator->Allocate();
	//CreateSampler();

	for (int32 i = 0; i < 2; i++)
	{
		FrameData& frame_data = mFrameData[i];

		frame_data.mSrvGpuDescriptorAllocator = new GpuDescriptorAllocator();
		frame_data.mSrvGpuDescriptorAllocator->Init(this, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV, 8);

		frame_data.mFence = CreateFence();
	}

	const int32 NumQueueTypes = static_cast<int32>(ERenderQueueType::Count);

	Queues.SetNum(NumQueueTypes);

	for (int32 i = 0; i < NumQueueTypes; i++)
	{
		RenderCommandQueue* CommandQueue = new RenderCommandQueue(static_cast<ERenderQueueType>(i));

		CommandQueue->Create(this);
		Queues[i] = CommandQueue;
	}

	mTextureAllocator = new RenderTextureAllocator(this);

	mRenderUploadBuffer = new RenderUploadBuffer();
	mRenderUploadBuffer->Init(this, 134217728);

	bInitialised = true;

	return true;
}

TComPtr<ID3D12DescriptorHeap> RenderDevice::CreateDescriptorHeap(D3D12_DESCRIPTOR_HEAP_DESC* inDesc)
{
	TComPtr<ID3D12DescriptorHeap> descriptor_heap;

	if (SUCCEEDED(mD3D12Device4->CreateDescriptorHeap(inDesc, IID_PPV_ARGS(&descriptor_heap))) == false)
	{
		return nullptr;
	}

	return descriptor_heap;
}

DescriptorHandleCPU RenderDevice::CreateRenderTargetView(TComPtr<ID3D12Resource> Resource)
{
	ID3D12Resource* RenderTargetView = nullptr;

	DescriptorHandleCPU Handle = RenderTargetViewDescriptorAllocator->Allocate();

	mD3D12Device4->CreateRenderTargetView(Resource, nullptr, Handle);

	return Handle;
}

ID3D12CommandAllocator* RenderDevice::CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE Type)
{
	ID3D12CommandAllocator* CommandAllocator = nullptr;

	if (SUCCEEDED(mD3D12Device4->CreateCommandAllocator(Type, IID_PPV_ARGS(&CommandAllocator))) == false)
	{
		return nullptr;
	}

	return CommandAllocator;
}

ID3D12GraphicsCommandList* RenderDevice::CreateCommandList(ID3D12CommandAllocator* CommandAllocator, D3D12_COMMAND_LIST_TYPE Type)
{
	ID3D12GraphicsCommandList* CommandList = nullptr;

	if (SUCCEEDED(mD3D12Device4->CreateCommandList(0, Type, CommandAllocator, nullptr, IID_PPV_ARGS(&CommandAllocator))) == false)
	{
		return nullptr;
	}

	return CommandList;
}

void RenderDevice::FreeDescriptor(DescriptorHandleCPU Handle)
{
	RenderTargetViewDescriptorAllocator->Free(Handle);
}

ID3D12Fence* RenderDevice::CreateFence()
{
	ID3D12Fence* D3D12Fence = nullptr;

	if (SUCCEEDED(mD3D12Device4->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&D3D12Fence))) == false)
	{
		return nullptr;
	}

	return D3D12Fence;
}

//RenderCommandAllocator* RenderDevice::ObtainCommandAllocator(ERenderQueueType QueueType)
//{
//	RenderCommandQueue* Queue = Queues[static_cast<uint32>(QueueType)];
//
//	RenderCommandAllocator* CommandAllocator = Queue->CreateCommandList();
//
//	return CommandAllocator;
//}

RenderCommandList* RenderDevice::ObtainCommandList(RenderCommandAllocator* CommandAllocator)
{
	RenderCommandQueue* Queue = Queues[static_cast<uint32>(CommandAllocator->GetQueueType())];

	return Queue->CreateCommandList();
}

void RenderDevice::WaitForGpuIdle()
{
	for (int32 i = 0; i < Queues.Num(); i++)
	{
		Queues[i]->WaitUntilIdle();
	}
}

D3D12Resource* RenderDevice::CreateResource(ERenderHeapType HeapType, uint64 Size, D3D12_RESOURCE_STATES inInitialState)
{
	D3D12_RESOURCE_DESC buffer_resource_desc = {};

	buffer_resource_desc.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
	buffer_resource_desc.Alignment = 0;
	buffer_resource_desc.Width = Size;
	buffer_resource_desc.Height = 1;
	buffer_resource_desc.DepthOrArraySize = 1;
	buffer_resource_desc.MipLevels = 1;
	buffer_resource_desc.Format = DXGI_FORMAT_UNKNOWN;
	buffer_resource_desc.SampleDesc.Count = 1;
	buffer_resource_desc.SampleDesc.Quality = 0;
	buffer_resource_desc.Layout = D3D12_TEXTURE_LAYOUT_ROW_MAJOR;
	buffer_resource_desc.Flags = D3D12_RESOURCE_FLAG_NONE;

	D3D12_HEAP_PROPERTIES heap_properties = {};

	heap_properties.Type = RenderHeapTypeToD3D12HeapType(HeapType);
	heap_properties.CPUPageProperty = D3D12_CPU_PAGE_PROPERTY_UNKNOWN;
	heap_properties.MemoryPoolPreference = D3D12_MEMORY_POOL_UNKNOWN;
	heap_properties.CreationNodeMask = 1;
	heap_properties.VisibleNodeMask = 1;

	D3D12Resource* resource = new D3D12Resource();

	TComPtr<ID3D12Resource> d3d12_resource;

	mD3D12Device4->CreateCommittedResource(&heap_properties, D3D12_HEAP_FLAG_NONE, &buffer_resource_desc, inInitialState, nullptr, IID_PPV_ARGS(&d3d12_resource));

	resource->SetResource(d3d12_resource);

	mResources.Add(resource);

	return resource;
}

void RenderDevice::CreateSampler()
{
	D3D12_SAMPLER_DESC sampler_desc = {};

	sampler_desc.AddressU = D3D12_TEXTURE_ADDRESS_MODE_CLAMP;
	sampler_desc.AddressV = D3D12_TEXTURE_ADDRESS_MODE_CLAMP;
	sampler_desc.AddressW = D3D12_TEXTURE_ADDRESS_MODE_CLAMP;
	sampler_desc.Filter = D3D12_FILTER_MIN_MAG_MIP_LINEAR;
	sampler_desc.MinLOD = 0.0f;
	sampler_desc.MaxLOD = 1.0f;

	mD3D12Device4->CreateSampler(&sampler_desc, mStaticSampler);
}

TComPtr<ID3D12PipelineState> RenderDevice::CreatePipelineState(const D3D12PipelineStateStream* PipelineStream)
{
	D3D12_PIPELINE_STATE_STREAM_DESC streamDesc = {};

	streamDesc.SizeInBytes = sizeof(D3D12PipelineStateStream);
	streamDesc.pPipelineStateSubobjectStream = (void*)PipelineStream;

	TComPtr<ID3D12PipelineState> pipeline_state;

	mD3D12Device4->CreatePipelineState(&streamDesc, IID_PPV_ARGS(&pipeline_state));

	mPipelineStates.Add(pipeline_state);

	return pipeline_state;
}

void RenderDevice::BeginFrame()
{
	FrameData& frame_data = mFrameData[GetFrameDataIndex()];

	// Wait to ensure we have finished before we reset an in-use frame.
	while (frame_data.mFence->GetCompletedValue() < frame_data.mSubmissionFenceValue)
	{
		_mm_pause();
	}

	frame_data.mSubmissionFenceValue = 0;

	GetFrameData().mSrvGpuDescriptorAllocator->Reset();
}

void RenderDevice::EndFrame()
{
	FrameData& frame_data = mFrameData[GetFrameDataIndex()];

	// Add fence to know this frame's data has been used.
	frame_data.mSubmissionFenceValue = GetQueue(ERenderQueueType::Direct)->GetSubmissionFenceValue();
	frame_data.mFence = GetQueue(ERenderQueueType::Direct)->GetSubmissionFence();

	mFrameCount++;
}

void RenderDevice::OnDeviceRemoved()
{
	GCrash("Device removed!");
}
