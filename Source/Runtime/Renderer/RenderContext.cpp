#include "Renderer/RenderContext.h"

#include "Renderer/D3D12RenderTargetView.h"
#include "Renderer/RenderCommandList.h"
#include "Renderer/RenderCommandQueue.h"
#include "Renderer/RenderDevice.h"
#include "Renderer/RenderTexture.h"

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

void RenderCommandContext::SetRenderTarget(RenderTexture* Texture)
{
	RenderTarget = Texture;

	GetCommandList()->AddTransitionBarrier(RenderTarget->GetResource(), D3D12_RESOURCE_STATE_PRESENT, D3D12_RESOURCE_STATE_RENDER_TARGET);

	float ClearColour[4] = {0, 0, 0, 0};
	uint32 ClearRectCount = 0;
	D3D12_RECT* ClearRects = nullptr;

	GetGraphicsCommandList()->ClearRenderTargetView(RenderTarget->GetRenderTargetViewDescriptor(), ClearColour, ClearRectCount, ClearRects);
}

void RenderCommandContext::TransitionResource(ID3D12Resource* Resource, D3D12_RESOURCE_STATES Before, D3D12_RESOURCE_STATES After)
{
	GetCommandList()->AddTransitionBarrier(Resource, Before, After);
}

void RenderCommandContext::FlushCommands()
{
	CommandList->Close();

	Device->GetQueue(ERenderQueueType::Direct)->ExecuteCommandList(GetGraphicsCommandList());

	CommandList = nullptr;

	//RenderFence Fence;

	//Fence.Signal();
	//Fence.Wait();
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

	CommandList->Reset();
}

void RenderCommandContext::CloseCommandList()
{
	CommandList->Close();

	CommandList = nullptr;
}
