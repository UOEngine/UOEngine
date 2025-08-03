#include "RenderCommandList.h"

#include "RenderCommandAllocator.h"
#include "RenderCommandQueue.h"
#include "RenderDevice.h"

RenderCommandList::RenderCommandList(ERenderQueueType inQueueType, RenderDevice* inDevice)
{
	mCommandList = nullptr;
	bClosed = true;
	mQueueType = inQueueType;
	mCommandAllocator = nullptr;

	D3D12_COMMAND_LIST_TYPE		Type;

	switch (inQueueType)
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

	switch (mQueueType)
	{
		case ERenderQueueType::Direct:
		case ERenderQueueType::Async:
		case ERenderQueueType::Copy:
		{
			HRESULT result = inDevice->GetDevice()->CreateCommandList1(0, Type, D3D12_COMMAND_LIST_FLAG_NONE, IID_PPV_ARGS(&mCommandList));
			
			if (FAILED(result))
			{
				GAssert(false);
			}

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

	if (FAILED(mCommandList->Close()))
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

	mCommandList->ResourceBarrier(1, &Barrier);
}

void RenderCommandList::Reset(RenderCommandAllocator* inCommandAllocator)
{
	GAssert(IsClosed());

	mCommandList->Reset(inCommandAllocator->GetHandle(), nullptr);

	mCommandAllocator = inCommandAllocator;
	bClosed = false;
}

RenderCommandAllocator* RenderCommandList::GetAndClearCommandAllocator()
{
	RenderCommandAllocator* command_allocator = mCommandAllocator;

	mCommandAllocator = nullptr;

	return command_allocator;
}

void RenderCommandList::CopyTextureRegion()
{
	GAssert(IsOpen());
}

