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

	void									ExecuteCommandList();

	ID3D12CommandQueue*						GetQueue() const			{return CommandQueue;}

	RenderDevice*							GetDevice() const			{return Device;}

	RenderCommandAllocator*					GetFreeCommandAllocator();
	RenderCommandList*						GetCommandList() const		{return CommandList;}

private:

	ID3D12CommandQueue*						CommandQueue;
	ERenderQueueType						QueueType;

	RenderDevice*							Device;

	struct CommandAllocatorEntry
	{
		RenderCommandAllocator*	CommandAllocator = nullptr;
		ID3D12Fence*			Fence = nullptr;
		uint64					FenceValue = 0;
	};

	TArray<CommandAllocatorEntry>			CommandAllocatorQueue;

	RenderCommandList*						CommandList;

	// Fence for the queue doing work.
	ID3D12Fence*							Fence;
	uint64									FenceValue;
	HANDLE									FenceEvent;

};
