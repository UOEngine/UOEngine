#pragma once

struct ID3D12RootSignature;

template <D3D12_PIPELINE_STATE_SUBOBJECT_TYPE SubObjectType, typename DescType>
struct alignas(void*) TD3D12PipelineStateStreamSubObject
{
private:
	D3D12_PIPELINE_STATE_SUBOBJECT_TYPE mType = SubObjectType;

public:
	DescType							mDesc = {};
};

class D3D12PipelineStateStream
{
public:

	void SetRootSignature(ID3D12RootSignature* RootSignature)
	{
		mRootSignature.mDesc = RootSignature;
	}

	void SetVertexShader(void* ByteCode, uint32 Length)
	{
		mVS.mDesc.pShaderBytecode = ByteCode;
		mVS.mDesc.BytecodeLength = Length;
	}

	void SetPixelShader(void* ByteCode, uint32 Length)
	{
		mPS.mDesc.pShaderBytecode = ByteCode;
		mPS.mDesc.BytecodeLength = Length;
	}

	void SetBlendState()
	{

		D3D12_RENDER_TARGET_BLEND_DESC rt_blend_desc = {};

		rt_blend_desc.BlendEnable = true;
		rt_blend_desc.SrcBlend = D3D12_BLEND_SRC_ALPHA;
		rt_blend_desc.DestBlend = D3D12_BLEND_INV_SRC_ALPHA;
		rt_blend_desc.BlendOp = D3D12_BLEND_OP_ADD;

		rt_blend_desc.SrcBlendAlpha = D3D12_BLEND_ONE;
		rt_blend_desc.DestBlendAlpha = D3D12_BLEND_ZERO;
		rt_blend_desc.BlendOpAlpha = D3D12_BLEND_OP_ADD;

		rt_blend_desc.RenderTargetWriteMask = D3D12_COLOR_WRITE_ENABLE_ALL;

		mBlend.mDesc.RenderTarget[0] = rt_blend_desc;
	}

	void SetInputLayout()
	{
		mInputLayout.mDesc.NumElements = 0;
		mInputLayout.mDesc.pInputElementDescs = nullptr;
	}

	void SetRasterizer()
	{
		mRasterizer.mDesc.CullMode = D3D12_CULL_MODE_BACK;
		mRasterizer.mDesc.FillMode = D3D12_FILL_MODE_SOLID;
		mRasterizer.mDesc.FrontCounterClockwise = true;
	}

	void SetPrimitiveTopology()
	{
		mPrimitiveTopologyType.mDesc = D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;
	}

	void SetRenderTargetFormat(DXGI_FORMAT Format)
	{
		mRTVFormats.mDesc.NumRenderTargets = 1;
		mRTVFormats.mDesc.RTFormats[0] = Format;
	}

private:
	
	TD3D12PipelineStateStreamSubObject<D3D12_PIPELINE_STATE_SUBOBJECT_TYPE_ROOT_SIGNATURE,			ID3D12RootSignature*>			mRootSignature;
	TD3D12PipelineStateStreamSubObject<D3D12_PIPELINE_STATE_SUBOBJECT_TYPE_VS,						D3D12_SHADER_BYTECODE>			mVS;
	TD3D12PipelineStateStreamSubObject<D3D12_PIPELINE_STATE_SUBOBJECT_TYPE_PS,						D3D12_SHADER_BYTECODE>			mPS;
	TD3D12PipelineStateStreamSubObject<D3D12_PIPELINE_STATE_SUBOBJECT_TYPE_BLEND,					D3D12_BLEND_DESC>				mBlend;
	TD3D12PipelineStateStreamSubObject<D3D12_PIPELINE_STATE_SUBOBJECT_TYPE_RASTERIZER,				D3D12_RASTERIZER_DESC>			mRasterizer;
	TD3D12PipelineStateStreamSubObject<D3D12_PIPELINE_STATE_SUBOBJECT_TYPE_DEPTH_STENCIL1,			D3D12_DEPTH_STENCIL_DESC1>		mDepthStencil;
	TD3D12PipelineStateStreamSubObject<D3D12_PIPELINE_STATE_SUBOBJECT_TYPE_INPUT_LAYOUT,			D3D12_INPUT_LAYOUT_DESC>		mInputLayout;
	TD3D12PipelineStateStreamSubObject<D3D12_PIPELINE_STATE_SUBOBJECT_TYPE_PRIMITIVE_TOPOLOGY,		D3D12_PRIMITIVE_TOPOLOGY_TYPE>	mPrimitiveTopologyType;
	TD3D12PipelineStateStreamSubObject<D3D12_PIPELINE_STATE_SUBOBJECT_TYPE_RENDER_TARGET_FORMATS,	D3D12_RT_FORMAT_ARRAY>			mRTVFormats;
};