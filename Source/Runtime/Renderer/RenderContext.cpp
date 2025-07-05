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
	GAssert(CommandList != nullptr);

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
	static int32 count = 0;

	count++;

	if (count % 3 == 0)
	{
		mDevice->GetSrvGpuDescriptorAllocator()->Reset();
	}

	DescriptorTable table = mDevice->GetSrvGpuDescriptorAllocator()->Allocate();

	mDevice->GetDevice()->CopyDescriptorsSimple(1, table.mCpuHandle, inTexture->GetSrv(), D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);

	ID3D12DescriptorHeap* heaps[] = {mDevice->GetSrvGpuDescriptorAllocator()->GetHeap()};
	//TComPtr<ID3D12DescriptorHeap> heap = mDevice->GetSrvGpuDescriptorAllocator()->GetHeap();

	CommandList->GetGraphicsCommandList()->SetDescriptorHeaps(1, heaps);
	CommandList->GetGraphicsCommandList()->SetGraphicsRootDescriptorTable(0, table.mGpuHandle);
}

void RenderCommandContext::FlushCommands()
{
	CloseCommandList();

	mDevice->GetQueue(QueueType)->ExecuteCommandList();
}

void RenderCommandContext::Draw()
{
	GetGraphicsCommandList()->DrawInstanced(4, 1, 0, 0);
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
	CommandList = mDevice->GetQueue(QueueType)->GetCommandList();

	CommandList->Reset();
}

void RenderCommandContext::CloseCommandList()
{
	CommandList->Close();

	CommandList = nullptr;
}
