#pragma once

#include "Core/Containers/Array.h"
#include "Core/Types.h"

class ID3D12Fence;
class RenderCommandAllocator;
class RenderCommandList;
class RenderDevice;
class RenderFence;
enum class ERenderQueueType: uint8;
struct ID3D12CommandList;
struct ID3D12CommandQueue;

using HANDLE = void*;

class RenderCommandQueue
{
public:
											RenderCommandQueue(ERenderQueueType InQueueType);

	void									Create(RenderDevice* Device);

	void									WaitUntilIdle();

	void									ExecuteCommandList(RenderCommandList* inCommandList);

	ID3D12CommandQueue*						GetQueue() const			{return mCommandQueue;}

	RenderDevice*							GetDevice() const			{return mDevice;}

	//RenderCommandAllocator*					GetFreeCommandAllocator();
	//RenderCommandList*						GetCommandList();

	RenderCommandList*						CreateCommandList();

private:

	ID3D12CommandQueue*						mCommandQueue;
	ERenderQueueType						mQueueType;

	RenderDevice*							mDevice;

	struct CommandAllocatorEntry
	{
		RenderCommandAllocator*	CommandAllocator = nullptr;
		RenderCommandList*		mCommandList;
		ID3D12Fence*			Fence = nullptr;
		uint64					FenceValue = 0;
	};

	//TArray<CommandAllocatorEntry>			CommandAllocatorQueue;

	RenderCommandList*						mActiveCommandList;

	TArray<RenderCommandList*>				mCommandLists;

	TArray<CommandAllocatorEntry>			mFreeCommandAllocators;

	// Fence for the queue doing work.
	ID3D12Fence*							Fence;
	uint64									FenceValue;
	HANDLE									FenceEvent;

};
