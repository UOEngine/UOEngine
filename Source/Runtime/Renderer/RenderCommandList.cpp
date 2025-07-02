#include "Renderer/RenderCommandList.h"

#include "Renderer/RenderCommandAllocator.h"
#include "Renderer/RenderCommandQueue.h"
#include "Renderer/RenderDevice.h"

RenderCommandList::RenderCommandList(RenderCommandAllocator* InCommandAllocator)
{
	CommandAllocator = InCommandAllocator;
	CommandList = nullptr;
	bClosed = true;

	QueueType = CommandAllocator->GetQueueType();

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

	switch (QueueType)
	{
		case ERenderQueueType::Direct:
		case ERenderQueueType::Async:
		case ERenderQueueType::Copy:
		{
			if (FAILED(CommandAllocator->GetDevice()->GetDevice()->CreateCommandList(0, Type, CommandAllocator->GetHandle(), nullptr, IID_PPV_ARGS(&CommandList))))
			{
				GAssert(false);
			}

			bClosed = false;

			Close();

			break;
		}

		default:
		{
			GAssert(false);
		}
	}
}

void RenderCommandList::Close()
{
	GAssert(IsOpen());

	if (FAILED(CommandList->Close()))
	{
		GAssert(false);
	}

	bClosed = true;
}

void RenderCommandList::AddTransitionBarrier(ID3D12Resource* Resource, D3D12_RESOURCE_STATES Before, D3D12_RESOURCE_STATES After)
{
	D3D12_RESOURCE_BARRIER Barrier = {};

	Barrier.Type = D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
	Barrier.Flags = D3D12_RESOURCE_BARRIER_FLAG_NONE;
	Barrier.Transition.pResource = Resource;
	Barrier.Transition.StateBefore = Before;
	Barrier.Transition.StateAfter = After;
	Barrier.Transition.Subresource = 0;

	CommandList->ResourceBarrier(1, &Barrier);
}

void RenderCommandList::Reset()
{
	GAssert(IsClosed());

	RenderCommandAllocator* FreeAllocator = CommandAllocator->GetDevice()->GetQueue(QueueType)->GetFreeCommandAllocator();

	//CommandList->Reset(FreeAllocator->GetHandle(), nullptr);
	//CommandAllocator->Reset();
	CommandList->Reset(CommandAllocator->GetHandle(), nullptr);

	//CommandAllocator = FreeAllocator;

	bClosed = false;
}

void RenderCommandList::CopyTextureRegion()
{
	GAssert(IsOpen());
}

