#include "Renderer/RenderCommandQueue.h"

#include <d3d12.h>

#include "Core/Assert.h"
#include "Renderer/RenderCommandAllocator.h"
#include "Renderer/RenderCommandList.h"
#include "Renderer/RenderDevice.h"

RenderCommandQueue::RenderCommandQueue(ERenderQueueType InQueueType)
{
	CommandQueue = nullptr;
	Fence = nullptr;
	QueueType = InQueueType;
	CommandAllocator = nullptr;
	CommandList = nullptr;
}

void RenderCommandQueue::Create(RenderDevice* InDevice)
{
	Device = InDevice;

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

	D3D12_COMMAND_QUEUE_DESC QueueDesc = {};

	QueueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
	QueueDesc.Type = Type;
	QueueDesc.Priority = D3D12_COMMAND_QUEUE_PRIORITY_NORMAL;

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

RenderCommandAllocator* RenderCommandQueue::GetFreeCommandAllocator()
{
	if (CommandAllocator == nullptr)
	{
		CommandAllocator = new RenderCommandAllocator(Device, QueueType);
	}

	return CommandAllocator;
}

RenderCommandList* RenderCommandQueue::GetFreeCommandList()
{
	if (CommandList == nullptr)
	{
		CommandList = new RenderCommandList(CommandAllocator);
	}

	return CommandList;
}
