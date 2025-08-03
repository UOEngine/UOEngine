#include "RenderCommandQueue.h"

#include <d3d12.h>

#include "Core/Assert.h"
#include "Memory/Memory.h"
#include "RenderCommandAllocator.h"
#include "RenderCommandList.h"
#include "RenderDevice.h"
#include "RenderFence.h"

RenderCommandQueue::RenderCommandQueue(ERenderQueueType InQueueType)
{
	mCommandQueue = nullptr;
	mQueueType = InQueueType;
	mActiveCommandList = nullptr;

	mFence = nullptr;
	mSubmissionFenceValue = 0;
	FenceEvent = INVALID_HANDLE_VALUE;
}

void RenderCommandQueue::Create(RenderDevice* InDevice)
{
	mDevice = InDevice;

	D3D12_COMMAND_LIST_TYPE		Type = RenderQueueTypeToCommandListType(mQueueType);

	D3D12_COMMAND_QUEUE_DESC QueueDesc = {};

	QueueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
	QueueDesc.Type = Type;
	QueueDesc.Priority = D3D12_COMMAND_QUEUE_PRIORITY_NORMAL;

	if (SUCCEEDED(mDevice->GetDevice()->CreateCommandQueue(&QueueDesc, IID_PPV_ARGS(&mCommandQueue))) == false)
	{
		GAssert(false);
	}

	mCommandQueue->SetName(TEXT("CommandQueue"));

	//for (int32 i = 0; i < CommandAllocatorQueue.GetCapacity(); i++)
	//{
	//	CommandAllocatorQueue.Add(CommandAllocatorEntry{});
	//}

	mFence = mDevice->CreateFence();
	FenceEvent = ::CreateEvent(nullptr, FALSE, FALSE, nullptr);

	//mActiveCommandList = new RenderCommandList(GetFreeCommandAllocator());
}

void RenderCommandQueue::WaitUntilIdle()
{
	// Dirty but for now ...

	TComPtr<ID3D12Fence> wait_until_idle_fence =  mDevice->CreateFence();

	mCommandQueue->Signal(wait_until_idle_fence, 1);

	while (wait_until_idle_fence->GetCompletedValue() < 1)
	{
		
	}
}

void RenderCommandQueue::ExecuteCommandList(RenderCommandList* inCommandList)
{
	GAssert(inCommandList != nullptr);
	GAssert(inCommandList->IsClosed());

	ID3D12CommandList* const command_lists[] = { inCommandList->GetGraphicsCommandList() };

	mCommandQueue->ExecuteCommandLists(1, command_lists);

	mSubmissionFenceValue++;

	mCommandQueue->Signal(mFence, mSubmissionFenceValue);

	CommandAllocatorEntry new_entry;

	new_entry.mCommandList = inCommandList;
	new_entry.FenceValue = mSubmissionFenceValue;
	new_entry.CommandAllocator = inCommandList->GetAndClearCommandAllocator();

	mFreeCommandAllocators.Add(new_entry);

	//for (int32 i = 0; i < CommandAllocatorQueue.Num(); i++)
	//{
	//	CommandAllocatorEntry& Entry = CommandAllocatorQueue[i];

	//	if (Entry.CommandAllocator == mActiveCommandList->GetCommandAllocator())
	//	{
	//		Entry.FenceValue = FenceValue;
	//	}
	//}

}

//RenderCommandAllocator* RenderCommandQueue::GetFreeCommandAllocator()
//{
//	for (int32 i = 0; i < CommandAllocatorQueue.Num(); i++)
//	{
//		CommandAllocatorEntry& Entry = CommandAllocatorQueue[i];
//
//		if (Entry.CommandAllocator == nullptr)
//		{
//			Entry.CommandAllocator = new RenderCommandAllocator(Device, QueueType);
//
//			Entry.Fence = Device->CreateFence();
//
//			CommandAllocatorQueue[i] = Entry;
//
//			return Entry.CommandAllocator;
//		}
//
//		if (Fence->GetCompletedValue() >= Entry.FenceValue)
//		{
//			Entry.CommandAllocator->Reset();
//
//			return Entry.CommandAllocator;
//		}
//	}
//
//	GAssert(false);
//
//	return nullptr;
//}

//RenderCommandList* RenderCommandQueue::GetCommandList()
//{
//	return mActiveCommandList;
//}

RenderCommandList* RenderCommandQueue::CreateCommandList()
{
	RenderCommandList* command_list = nullptr;
	RenderCommandAllocator* command_allocator = nullptr;

	int32 free_command_allocator_index = -1;

	for (int32 i = 0; i < mFreeCommandAllocators.Num(); i++)
	{
		if (mFence->GetCompletedValue() >= mFreeCommandAllocators[i].FenceValue)
		{
			free_command_allocator_index = i;

			break;
		}
	}

	if (free_command_allocator_index >= 0)
	{
		CommandAllocatorEntry& free_entry = mFreeCommandAllocators[free_command_allocator_index];

		command_list = free_entry.mCommandList;
		command_allocator = free_entry.CommandAllocator;

		mFreeCommandAllocators.RemoveAt(free_command_allocator_index, false);
	}

	if (command_allocator != nullptr)
	{
		command_allocator->Reset();
		command_list->Reset(command_allocator);

		return command_list;
	}

	RenderCommandAllocator* new_command_allocator = new RenderCommandAllocator(mDevice, mQueueType);
	RenderCommandList* new_command_list = new RenderCommandList(mQueueType, mDevice);

	new_command_list->Reset(new_command_allocator);

	return new_command_list;
}
