#include "Renderer/RenderContext.h"

#include "Renderer/D3D12Resource.h"
#include "Renderer/D3D12RenderTargetView.h"
#include "Renderer/GpuDescriptorAllocator.h"
#include "Renderer/RenderCommandList.h"
#include "Renderer/RenderCommandQueue.h"
#include "Renderer/RenderDevice.h"
#include "Renderer/RenderTextureAllocator.h"
#include "Renderer/RenderTexture.h"

RenderCommandContext::RenderCommandContext(RenderDevice* InDevice)
{
	mDevice = InDevice;

	mCommandList = nullptr;
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
	mDevice->GetTextureAllocator()->FlushPendingUploads(this);

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

	GetCommandList()->AddTransitionBarrier(RenderTarget->GetResource()->Get(), D3D12_RESOURCE_STATE_PRESENT, D3D12_RESOURCE_STATE_RENDER_TARGET);

	float ClearColour[4] = { 0.3f, 0.3f, 0.3f, 0.f };
	uint32 ClearRectCount = 0;
	D3D12_RECT* ClearRects = nullptr;

	GetGraphicsCommandList()->ClearRenderTargetView(RenderTarget->GetRenderTargetViewDescriptor(), ClearColour, ClearRectCount, ClearRects);

	D3D12_CPU_DESCRIPTOR_HANDLE backbuffer_rtv = RenderTarget->GetRenderTargetViewDescriptor();

	GetGraphicsCommandList()->OMSetRenderTargets(1, &backbuffer_rtv, false, nullptr);
}

void RenderCommandContext::SetPipelineState(ID3D12PipelineState* PipelineState, ID3D12RootSignature* RootSignature)
{
	GetGraphicsCommandList()->SetPipelineState(PipelineState);
	GetGraphicsCommandList()->SetGraphicsRootSignature(RootSignature);
	GetGraphicsCommandList()->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP);
}

void RenderCommandContext::TransitionResource(ID3D12Resource* Resource, D3D12_RESOURCE_STATES Before, D3D12_RESOURCE_STATES After)
{
	GAssert(mCommandList != nullptr);

	GetCommandList()->AddTransitionBarrier(Resource, Before, After);
}

void RenderCommandContext::SetViewport(uint32 Width, uint32 Height)
{
	D3D12_VIEWPORT viewport = {
		.TopLeftX = 0,
		.TopLeftY = 0,
		.Width = (float)Width,
		.Height = (float)Height,
		.MinDepth = 0.0f,
		.MaxDepth = 1.0f
	};

	D3D12_RECT scissor = {
		.left = 0,
		.top = 0,
		.right = (int32)Width,
		.bottom = (int32)Height,
	};

	GetGraphicsCommandList()->RSSetViewports(1, &viewport);
	GetGraphicsCommandList()->RSSetScissorRects(1, &scissor);
}

void RenderCommandContext::SetShaderBindingData(RenderTexture* inTexture)
{
	DescriptorTable			table = mDevice->GetSrvGpuDescriptorAllocator()->Allocate();
	ID3D12DescriptorHeap*	heaps[] = {mDevice->GetSrvGpuDescriptorAllocator()->GetHeap()};

	mCommandList->GetGraphicsCommandList()->SetDescriptorHeaps(1, heaps);

	mDevice->GetDevice()->CopyDescriptorsSimple(1, table.mCpuHandle, inTexture->GetSrv(), D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);

	mCommandList->GetGraphicsCommandList()->SetGraphicsRootDescriptorTable(0, table.mGpuHandle);
}

void RenderCommandContext::FlushCommands()
{
	RenderCommandList* command_list = mCommandList;

	CloseCommandList();

	mDevice->GetQueue(QueueType)->ExecuteCommandList(command_list);
}

void RenderCommandContext::Draw()
{
	GetGraphicsCommandList()->DrawInstanced(4, 1, 0, 0);
}

RenderCommandList* RenderCommandContext::GetCommandList()
{
	if (mCommandList == nullptr)
	{
		OpenCommandList();
	}

	return mCommandList;
}

void RenderCommandContext::OpenCommandList()
{
	mCommandList = mDevice->GetQueue(QueueType)->CreateCommandList();
}

void RenderCommandContext::CloseCommandList()
{
	mCommandList->Close();

	mCommandList = nullptr;
}
