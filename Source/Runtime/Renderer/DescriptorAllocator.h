#pragma once

#include <d3d12.h>

#include "Core/Assert.h"
#include "Core/TComPtr.h"

class RenderDevice;

struct DescriptorHandleCPU : D3D12_CPU_DESCRIPTOR_HANDLE
{
	using D3D12Type = D3D12_CPU_DESCRIPTOR_HANDLE;
	static constexpr size_t Invalid = (uint64)-1;
	inline bool				IsValid() const { return ptr != Invalid; }
};

class DescriptorAllocator
{
public:

										DescriptorAllocator();

	void								Init(RenderDevice* Device, D3D12_DESCRIPTOR_HEAP_TYPE inHeapType, uint32 inNumDescriptors);

	DescriptorHandleCPU					Allocate();
	void								Free(DescriptorHandleCPU Handle);

private:

	struct FreeList
	{
		void					Init(int32 InCapacity)
								{
									GAssert(Data == nullptr);

									Data = new DescriptorHandleCPU[InCapacity];
									Capacity = InCapacity;
								}

		void					Push(DescriptorHandleCPU Handle)
								{
									GAssert(Data != nullptr);
									GAssert(Capacity >= Size + 1);

									Data[Size++] = Handle;
								}

		DescriptorHandleCPU		Pop() 
								{
									GAssert(Data != nullptr); 
									GAssert(Size > 0);

									return Data[--Size];
								}

		uint32					Size = 0;
		uint32					Capacity = 0;
		DescriptorHandleCPU*	Data = nullptr;
	};

	TComPtr<ID3D12DescriptorHeap>		DescriptorHeap;
	D3D12_DESCRIPTOR_HEAP_TYPE   		Type = D3D12_DESCRIPTOR_HEAP_TYPE_NUM_TYPES;
	uint32								Num;
	uint32								LastFreeIndex;
	uint32								HandleSize;
	FreeList							FreeHandles;

};
