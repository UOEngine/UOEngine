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

void RenderCommandContext::SetBindlessTextures(const TArray<RenderTexture*>& inTextures)
{
	mBindlessTextures = inTextures;
}

void RenderCommandContext::FlushCommands()
{
	RenderCommandList* command_list = mCommandList;

	CloseCommandList();

	mDevice->GetQueue(QueueType)->ExecuteCommandList(command_list);
}

void RenderCommandContext::Draw(uint32 inNumInstances)
{
	Bind();

	GetGraphicsCommandList()->DrawInstanced(4, inNumInstances, 0, 0);
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

	if (mShaderInstance == nullptr)
	{
		GAssert(false);
	}

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

			switch (binding_info.mType)
			{
				case EShaderBindingType::Texture:
				{
					// Mostly using bindless anyway but temp. 
					TArray<RenderTexture*> texture;

					texture.Add(slot.mTexture);

					SetTextures(shader_program_type, binding_index, texture);

					break;
				}

				case EShaderBindingType::StructuredBuffer:
				{
					SetBuffer(shader_program_type, binding_index, slot.mBuffer);

					break;
				}

				case EShaderBindingType::BindlessTexture:
				{
					SetTextures(shader_program_type, binding_index, mBindlessTextures);

					break;
				}

				default:
					GNotImplemented;
			}

			// Binding this always currently, but eventually only if changed.
			//if (mDirtySrvs[i] & (1 << binding_index))
			{
				mCommandList->GetGraphicsCommandList()->SetGraphicsRootDescriptorTable(binding_info.mRootParameterIndex, mGpuDescriptorsToBind[i][binding_index]);

				mDirtySrvs[i] &= ~(1 << binding_index);
			}

		}
	}

	mbRenderStateDirty = false;
}

void RenderCommandContext::SetTextures(EShaderType inShaderType, uint32 inSlot, const TArray<RenderTexture*>& inTextures)
{
	const uint32 num_textures = inTextures.Num();

	GAssert(num_textures > 0);

	DescriptorTable			table = mDevice->GetSrvGpuDescriptorAllocator(ERenderResourceLifetime::Frame)->Allocate(num_textures);
	ID3D12DescriptorHeap* heaps[] = { mDevice->GetSrvGpuDescriptorAllocator(ERenderResourceLifetime::Frame)->GetHeap() };

	mCommandList->GetGraphicsCommandList()->SetDescriptorHeaps(1, heaps);

	TArray<DescriptorHandleCPU> texture_srvs;


	texture_srvs.SetNum(num_textures);

	for (int32 i = 0; i < num_textures; i++)
	{
		const RenderTexture* texture = inTextures[i];

		GAssert(texture->GetSrv().IsValid());

		texture_srvs[i] = texture->GetSrv();
	}

	mDevice->GetDevice()->CopyDescriptorsSimple(num_textures, table.mCpuHandle, texture_srvs[0], D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);

	SetPendingGpuDescriptor(inShaderType, inSlot, table);
}

void RenderCommandContext::SetBuffer(EShaderType inProgramType, uint32 inSlot, RenderBuffer* inBuffer)
{
	GAssert(inBuffer != nullptr);

	DescriptorTable			table = mDevice->GetSrvGpuDescriptorAllocator(ERenderResourceLifetime::Frame)->Allocate();
	ID3D12DescriptorHeap* heaps[] = { mDevice->GetSrvGpuDescriptorAllocator(ERenderResourceLifetime::Frame)->GetHeap() };

	mCommandList->GetGraphicsCommandList()->SetDescriptorHeaps(1, heaps);

	mDevice->GetDevice()->CopyDescriptorsSimple(1, table.mCpuHandle, inBuffer->GetSrv(), D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);

	SetPendingGpuDescriptor(inProgramType, inSlot, table);

}

void RenderCommandContext::SetPendingGpuDescriptor(EShaderType inShaderType, uint32 inSlot, DescriptorTable inTable)
{
	mGpuDescriptorsToBind[static_cast<uint32>(inShaderType)][inSlot] = inTable.mGpuHandle;

	mDirtySrvs[static_cast<uint32>(inShaderType)] |= (1 << inSlot);
}
