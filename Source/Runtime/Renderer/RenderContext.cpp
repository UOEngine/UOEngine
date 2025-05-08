#include "Renderer/RenderContext.h"

#include "Renderer/D3D12RenderTargetView.h"
#include "Renderer/RenderCommandList.h"
#include "Renderer/RenderDevice.h"

RenderCommandContext::RenderCommandContext(RenderDevice* InDevice)
{
	Device = InDevice;

	CommandList = nullptr;
	CommandAllocator = nullptr;

	QueueType = ERenderQueueType::Direct;

	RenderTarget = nullptr;
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

void RenderCommandContext::SetRenderTarget(D3D12RenderTargetView* View)
{
	RenderTarget = View;

	GetCommandList()->AddTransitionBarrier(RenderTarget->GetResource(), D3D12_RESOURCE_STATE_PRESENT, D3D12_RESOURCE_STATE_RENDER_TARGET);

	float ClearColour[4] = {0, 0, 0, 0};
	uint32 ClearRectCount = 0;
	D3D12_RECT* ClearRects = nullptr;

	GetGraphicsCommandList()->ClearRenderTargetView(View->GetDescriptorHandleCPU(), ClearColour, ClearRectCount, ClearRects);
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
