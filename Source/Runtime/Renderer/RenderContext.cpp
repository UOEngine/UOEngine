#include "Renderer/RenderContext.h"

#include "Renderer/RenderCommandList.h"
#include "Renderer/RenderDevice.h"

RenderCommandContext::RenderCommandContext(RenderDevice* InDevice)
{
	Device = InDevice;

	CommandList = nullptr;
	CommandAllocator = nullptr;
}

void RenderCommandContext::Begin()
{
	OpenCommandList();
}

void RenderCommandContext::End()
{
	CloseCommandList();
}

void RenderCommandContext::BeginRenderPass()
{
}

void RenderCommandContext::EndRenderPass()
{

}

RenderCommandList* RenderCommandContext::GetCommandList()
{
	if (CommandList == nullptr)
	{
		OpenCommandList();
	}

	return CommandList;
}

void RenderCommandContext::OpenCommandList()
{
	if (CommandAllocator == nullptr)
	{
		CommandAllocator = Device->ObtainCommandAllocator(QueueType);
	}

	CommandList = Device->ObtainCommandList(CommandAllocator);
}

void RenderCommandContext::CloseCommandList()
{
	CommandList->Close();

	CommandList = nullptr;
}
