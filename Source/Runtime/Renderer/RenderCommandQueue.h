#pragma once

#include "Core/Types.h"

class ID3D12Fence;
class RenderCommandAllocator;
class RenderCommandList;
class RenderDevice;
enum class ERenderQueueType: uint8;
struct ID3D12CommandList;
struct ID3D12CommandQueue;

class RenderCommandQueue
{
public:
								RenderCommandQueue(ERenderQueueType InQueueType);

	void						Create(RenderDevice* Device);

	void						WaitUntilIdle();

	void						ExecuteCommandList(ID3D12CommandList* CommandList);

	ID3D12CommandQueue*			GetQueue() const {return CommandQueue;}

	RenderDevice*				GetDevice() const {return Device;}

	RenderCommandAllocator*		GetFreeCommandAllocator();
	RenderCommandList*			GetFreeCommandList();

private:

	ID3D12CommandQueue*			CommandQueue;
	ID3D12Fence*				Fence;
	ERenderQueueType			QueueType;

	RenderDevice*				Device;

	RenderCommandAllocator*		CommandAllocator;
	RenderCommandList*			CommandList;

};
