#pragma once

#include "Core/Types.h"

class ID3D12Fence;
class RenderDevice;
enum D3D12_COMMAND_LIST_TYPE;
enum class ERenderQueueType: uint8;
struct ID3D12CommandList;
struct ID3D12CommandQueue;

class RenderCommandQueue
{
public:
								RenderCommandQueue(ERenderQueueType InQueueType);

	void						Create(RenderDevice* Device, D3D12_COMMAND_LIST_TYPE InType);

	void						WaitUntilIdle();

	void						ExecuteCommandList(ID3D12CommandList* CommandList);

	ID3D12CommandQueue*			GetQueue() const {return CommandQueue;}

private:

	D3D12_COMMAND_LIST_TYPE		Type;
	ID3D12CommandQueue*			CommandQueue;
	ID3D12Fence*				Fence;

};
