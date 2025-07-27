#include "Renderer/RenderContext.h"

#include "Renderer/D3D12Resource.h"
#include "Renderer/D3D12RenderTargetView.h"
#include "Renderer/GpuDescriptorAllocator.h"
#include "Renderer/RenderBuffer.h"
#include "Renderer/RenderCommandList.h"
#include "Renderer/RenderCommandQueue.h"
#include "Renderer/RenderDevice.h"
#include "Renderer/RenderTextureAllocator.h"
#include "Renderer/RenderTexture.h"
#include "Renderer/Shader.h"
#include "Renderer/ShaderInstance.h"

RenderCommandContext::RenderCommandContext(RenderDevice* InDevice)
{
	mDevice = InDevice;

	mCommandList = nullptr;
	CommandAllocator = nullptr;

	QueueType = ERenderQueueType::Direct;

	RenderTarget = nullptr;

	mProjectionMatrix.SetToIdentity();

	mShaderInstance = nullptr;

	mbRenderStateDirty = true;
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

void RenderCommandContext::SetViewport(const Rect& inViewportRect)
{
	mViewportRect = inViewportRect;

	D3D12_VIEWPORT viewport = {
		.TopLeftX = 0,
		.TopLeftY = 0,
		.Width = (float)mViewportRect.Width(),
		.Height = (float)mViewportRect.Height(),
		.MinDepth = 0.0f,
		.MaxDepth = 1.0f
	};

	D3D12_RECT scissor = {
		.left = 0,
		.top = 0,
		.right = (int32)mViewportRect.Width(),
		.bottom = (int32)mViewportRect.Height(),
	};

	GetGraphicsCommandList()->RSSetViewports(1, &viewport);
	GetGraphicsCommandList()->RSSetScissorRects(1, &scissor);
}

//void RenderCommandContext::SetShaderBindingData(EShaderType inShaderType, RenderTexture* inTexture, uint32 inSlot)
//{
//	DescriptorTable			table = mDevice->GetSrvGpuDescriptorAllocator()->Allocate();
//	ID3D12DescriptorHeap*	heaps[] = {mDevice->GetSrvGpuDescriptorAllocator()->GetHeap()};
//
//	mCommandList->GetGraphicsCommandList()->SetDescriptorHeaps(1, heaps);
//
//	mDevice->GetDevice()->CopyDescriptorsSimple(1, table.mCpuHandle, inTexture->GetSrv(), D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
//
//	mGpuDescriptorsToBind[static_cast<uint32>(inShaderType)][inSlot] = table.mGpuHandle;
//
//	mbRenderStateDirty = true;
//}

void RenderCommandContext::SetShaderInstance(ShaderInstance* inShaderInstance)
{
	if (inShaderInstance == mShaderInstance)
	{
		return;
	}

	mShaderInstance = inShaderInstance;

	mbRenderStateDirty = true;
}

void RenderCommandContext::SetProjectionMatrix(const Matrix4x4& inMatrix)
{
	mProjectionMatrix = inMatrix;

	mCommandList->GetGraphicsCommandList()->SetGraphicsRoot32BitConstants(0, Matrix4x4::NumElements, &mProjectionMatrix, 0);
}

void RenderCommandContext::FlushCommands()
{
	RenderCommandList* command_list = mCommandList;

	CloseCommandList();

	mDevice->GetQueue(QueueType)->ExecuteCommandList(command_list);
}

void RenderCommandContext::Draw()
{
	Bind();

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

	Memory::MemZero(mDirtySrvs, sizeof(uint16) * static_cast<uint32>(EShaderType::Count));
}

void RenderCommandContext::CloseCommandList()
{
	mCommandList->Close();

	mCommandList = nullptr;
}

void RenderCommandContext::Bind()
{
	//if (mbRenderStateDirty == false)
	//{
	//	return;
	//}

	const uint32 num_shader_programs = static_cast<uint32>(EShaderType::Count);

	for (uint32 i = 0; i < num_shader_programs; i++)
	{
		EShaderType shader_program_type = static_cast<EShaderType>(i);

		Shader* shader = mShaderInstance->GetShader(shader_program_type);

		if (shader == nullptr)
		{
			continue;
		}

		const ShaderBindingInfo* shader_binding_info = shader->GetBindingInfo();
		const ShaderBoundData*	shader_bound_data = mShaderInstance->GetBoundData(shader_program_type);

		for (uint32 element = 0; element < shader_binding_info->mBindings.Num(); element++)
		{
			const ShaderBinding& binding_info = shader_binding_info->mBindings[element];

			uint32 binding_index = binding_info.mBindIndex;

			GAssert(binding_info.mType != EShaderBindingType::Invalid);

			if (binding_info.mType == EShaderBindingType::ConstantBuffer)
			{
				continue;
			}

			if (binding_info.mType == EShaderBindingType::Sampler)
			{
				continue;
			}

			const Slot& slot = shader_bound_data->mData[binding_index];

			uint32 hack_bind = 0;

			switch (binding_info.mType)
			{
				case EShaderBindingType::Texture:
				{
					SetTexture(binding_index, i, slot.mTexture);

					break;
				}

				case EShaderBindingType::StructuredBuffer:
				{
					SetBuffer(binding_index, i, slot.mBuffer);

					break;
				}

				default:
					GNotImplemented;
			}

			mCommandList->GetGraphicsCommandList()->SetGraphicsRootDescriptorTable(binding_info.mRootParameterIndex, mGpuDescriptorsToBind[i][binding_index]);

		}
	}

	mbRenderStateDirty = false;
}

void RenderCommandContext::SetTexture(uint32 inSlot, uint32 inProgramIndex, RenderTexture* inTexture)
{
	GAssert(inTexture != nullptr);

	DescriptorTable			table = mDevice->GetSrvGpuDescriptorAllocator()->Allocate();
	ID3D12DescriptorHeap* heaps[] = { mDevice->GetSrvGpuDescriptorAllocator()->GetHeap() };

	mCommandList->GetGraphicsCommandList()->SetDescriptorHeaps(1, heaps);

	mDevice->GetDevice()->CopyDescriptorsSimple(1, table.mCpuHandle, inTexture->GetSrv(), D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);

	mGpuDescriptorsToBind[inProgramIndex][inSlot] = table.mGpuHandle;
}

void RenderCommandContext::SetBuffer(uint32 inSlot, uint32 inProgramIndex, RenderBuffer* inBuffer)
{
	GAssert(inBuffer != nullptr);

	DescriptorTable			table = mDevice->GetSrvGpuDescriptorAllocator()->Allocate();
	ID3D12DescriptorHeap* heaps[] = { mDevice->GetSrvGpuDescriptorAllocator()->GetHeap() };

	mCommandList->GetGraphicsCommandList()->SetDescriptorHeaps(1, heaps);

	mDevice->GetDevice()->CopyDescriptorsSimple(1, table.mCpuHandle, inBuffer->GetSrv(), D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);

	mGpuDescriptorsToBind[inProgramIndex][inSlot] = table.mGpuHandle;

}
