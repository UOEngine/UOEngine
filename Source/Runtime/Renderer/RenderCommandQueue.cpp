#include "Renderer/RenderCommandQueue.h"

#include <d3d12.h>

#include "Core/Assert.h"
#include "Renderer/RenderDevice.h"

RenderCommandQueue::RenderCommandQueue(ERenderQueueType InQueueType)
{
	CommandQueue = nullptr;
	Type = D3D12_COMMAND_LIST_TYPE_DIRECT;
	Fence = nullptr;
}

void RenderCommandQueue::Create(RenderDevice* Device, D3D12_COMMAND_LIST_TYPE InType)
{
	D3D12_COMMAND_QUEUE_DESC QueueDesc = {};

	QueueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
	QueueDesc.Type = InType;
	QueueDesc.Priority = D3D12_COMMAND_QUEUE_PRIORITY_NORMAL;

	Type = InType;

	if (SUCCEEDED(Device->GetDevice()->CreateCommandQueue(&QueueDesc, IID_PPV_ARGS(&CommandQueue))) == false)
	{
		GAssert(false);
	}

	Fence = Device->CreateFence();
}

void RenderCommandQueue::WaitUntilIdle()
{

}

void RenderCommandQueue::ExecuteCommandList(ID3D12CommandList* CommandList)
{
	CommandQueue->ExecuteCommandLists(1, &CommandList);
}
