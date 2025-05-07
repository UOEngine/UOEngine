#include "Renderer/RenderCommandList.h"

#include "Renderer/RenderCommandAllocator.h"

RenderCommandList::RenderCommandList(RenderCommandAllocator* InCommandAllocator)
{
	CommandAllocator = InCommandAllocator;

	QueueType = CommandAllocator->GetQueueType();
}

void RenderCommandList::Close()
{

}

