#include "Renderer/RenderCommandAllocator.h"

#include "Core/Assert.h"

#include "Renderer/RenderDevice.h"

RenderCommandAllocator::RenderCommandAllocator(RenderDevice* InDevice, ERenderQueueType InQeueType)
{
	Device = InDevice;
	QueueType = InQeueType;

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

	if (FAILED(Device->GetDevice()->CreateCommandAllocator(Type, IID_PPV_ARGS(&CommandAllocator))))
	{
		GAssert(false);
	}
}

void RenderCommandAllocator::Reset()
{
	CommandAllocator->Reset();
}
