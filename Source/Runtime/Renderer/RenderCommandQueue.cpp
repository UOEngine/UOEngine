#include "Renderer/RenderCommandQueue.h"

#include <d3d12.h>

#include "Core/Assert.h"
#include "Memory/Memory.h"
#include "Renderer/RenderCommandAllocator.h"
#include "Renderer/RenderCommandList.h"
#include "Renderer/RenderDevice.h"
#include "Renderer/RenderFence.h"

RenderCommandQueue::RenderCommandQueue(ERenderQueueType InQueueType)
{
	CommandQueue = nullptr;
	QueueType = InQueueType;
	CommandList = nullptr;

	Fence = nullptr;
	FenceValue = 0;
	FenceEvent = INVALID_HANDLE_VALUE;
}

void RenderCommandQueue::Create(RenderDevice* InDevice)
{
	Device = InDevice;

	D3D12_COMMAND_LIST_TYPE		Type = RenderQueueTypeToCommandListType(QueueType);

	D3D12_COMMAND_QUEUE_DESC QueueDesc = {};

	QueueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
	QueueDesc.Type = Type;
	QueueDesc.Priority = D3D12_COMMAND_QUEUE_PRIORITY_NORMAL;

	if (SUCCEEDED(Device->GetDevice()->CreateCommandQueue(&QueueDesc, IID_PPV_ARGS(&CommandQueue))) == false)
	{
		GAssert(false);
	}

	CommandQueue->SetName(TEXT("CommandQueue"));

	CommandAllocatorQueue = TArray< CommandAllocatorEntry>(3);

	for (int32 i = 0; i < CommandAllocatorQueue.GetCapacity(); i++)
	{
		CommandAllocatorQueue.Add(CommandAllocatorEntry{});
	}

	Fence = Device->CreateFence();
	FenceEvent = ::CreateEvent(nullptr, FALSE, FALSE, nullptr);

	CommandList = new RenderCommandList(GetFreeCommandAllocator());
}

void RenderCommandQueue::WaitUntilIdle()
{
	// Dirty but for now ...

	ID3D12Fence* WaitUntilIdleFence =  Device->CreateFence();

	CommandQueue->Signal(WaitUntilIdleFence, 1);

	while (WaitUntilIdleFence->GetCompletedValue() < 1)
	{
		
	}

	WaitUntilIdleFence->Release();
}

void RenderCommandQueue::ExecuteCommandList()
{
	ID3D12CommandList* const CommandLists[] = {CommandList->GetGraphicsCommandList()};

	CommandQueue->ExecuteCommandLists(1, CommandLists);

	FenceValue++;

	CommandQueue->Signal(Fence, FenceValue);

	for (int32 i = 0; i < CommandAllocatorQueue.Num(); i++)
	{
		CommandAllocatorEntry& Entry = CommandAllocatorQueue[i];

		if (Entry.CommandAllocator == CommandList->GetCommandAllocator())
		{
			Entry.FenceValue = FenceValue;
		}
	}

}

RenderCommandAllocator* RenderCommandQueue::GetFreeCommandAllocator()
{
	for (int32 i = 0; i < CommandAllocatorQueue.Num(); i++)
	{
		CommandAllocatorEntry& Entry = CommandAllocatorQueue[i];

		if (Entry.CommandAllocator == nullptr)
		{
			Entry.CommandAllocator = new RenderCommandAllocator(Device, QueueType);

			Entry.Fence = Device->CreateFence();

			CommandAllocatorQueue[i] = Entry;

			return Entry.CommandAllocator;
		}

		if (Fence->GetCompletedValue() >= Entry.FenceValue)
		{
			Entry.CommandAllocator->Reset();

			return Entry.CommandAllocator;
		}
	}

	GAssert(false);

	return nullptr;
}
