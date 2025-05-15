#include "Renderer/RenderCommandQueue.h"

#include <d3d12.h>

#include "Core/Assert.h"
#include "Renderer/RenderCommandAllocator.h"
#include "Renderer/RenderCommandList.h"
#include "Renderer/RenderDevice.h"
#include "Renderer/RenderFence.h"

RenderCommandQueue::RenderCommandQueue(ERenderQueueType InQueueType)
{
	CommandQueue = nullptr;
	QueueType = InQueueType;
	CommandAllocator = nullptr;
	CommandList = nullptr;

	Fence = nullptr;
	FenceValue = 0;
	FenceEvent = INVALID_HANDLE_VALUE;
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
	FenceEvent = ::CreateEvent(nullptr, FALSE, FALSE, nullptr);
}

void RenderCommandQueue::WaitUntilIdle()
{
	FenceValue++;

	CommandQueue->Signal(Fence, FenceValue);

	if (Fence->GetCompletedValue() >= FenceValue)
	{
		return;
	}

	Fence->SetEventOnCompletion(FenceValue, FenceEvent);

	WaitForSingleObject(FenceEvent, INFINITE);
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
