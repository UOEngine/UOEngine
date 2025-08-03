#pragma once

#include <d3d12.h>

#include "Core/TComPtr.h"

class RenderDevice;
enum D3D12_DESCRIPTOR_HEAP_TYPE;
struct ID3D12DescriptorHeap;

struct DescriptorTable
{
	D3D12_GPU_DESCRIPTOR_HANDLE  mGpuHandle; // GPU handle to start of range
	D3D12_CPU_DESCRIPTOR_HANDLE  mCpuHandle; // Allocated CPU handle range
};

class GpuDescriptorAllocator
{
public:

	void								Init(RenderDevice* inRenderDevice, D3D12_DESCRIPTOR_HEAP_TYPE inHeapType, uint32 inNumDescriptors);

	DescriptorTable						Allocate(uint32 inCount = 1);
	void								Reset()		{mCurrentOffset = 0;}

	TComPtr<ID3D12DescriptorHeap>		GetHeap()	{return mDescriptorHeap;}
private:

	TComPtr<ID3D12DescriptorHeap>		mDescriptorHeap;
	uint32								mHandleSize = 0;
	uint32								mCurrentOffset = 0;
	uint32								mSize = 0;
	D3D12_DESCRIPTOR_HEAP_TYPE			mType = D3D12_DESCRIPTOR_HEAP_TYPE_NUM_TYPES;

};