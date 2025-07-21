#include "Renderer/Shader.h"

#include <d3d12.h>
#include <dxcapi.h>

#include "Core/FileReader.h"

Shader::Shader(EShaderType InType)
{
	mType = InType;
	mNumSignatureParameters = 0;
	mNumBoundResources = 0;
}

bool Shader::Load(const String& FilePath)
{
	FileHandle* shader_file = nullptr;

	GAssert(mType != EShaderType::Compute); // Not supported yet.

	if (FileDevice::Open(FilePath, shader_file) == false)
	{
		GAssert(false);

		return false;
	}
	
	TArray<uint8> shader_contents;

	shader_contents.SetNum(shader_file->GetSize());

	if (FileDevice::Read(shader_file, shader_contents.GetData()) == false)
	{
		GAssert(false);
	}

	FileDevice::Close(shader_file);

	GAssert(shader_contents.Num() > 0);

	TComPtr<IDxcUtils>			dxil_utils;
	TComPtr<IDxcCompiler3>		dxc_compiler;
	TComPtr<IDxcIncludeHandler>	dxc_include_header;

	// Initialize DXC components
	DxcCreateInstance(CLSID_DxcUtils, IID_PPV_ARGS(&dxil_utils));
	DxcCreateInstance(CLSID_DxcCompiler, IID_PPV_ARGS(&dxc_compiler));

	dxil_utils->CreateDefaultIncludeHandler(&dxc_include_header);

	TComPtr<IDxcBlobEncoding> dxc_source_blob;

	HRESULT create_blob_result = dxil_utils->CreateBlobFromPinned(shader_contents.GetData(), shader_contents.Num(), DXC_CP_UTF8, &dxc_source_blob);

	DxcBuffer source_buffer;

	GAssert(dxc_source_blob->GetBufferSize() > 0);

	source_buffer.Ptr = dxc_source_blob->GetBufferPointer();
	source_buffer.Size = dxc_source_blob->GetBufferSize();
	source_buffer.Encoding = DXC_CP_UTF8;

	TComPtr<IDxcResult> compile_result;

	LPCWSTR type_arg = mType == EShaderType::Vertex? L"vs_6_0" : L"ps_6_0";

	TArray<LPCWSTR> Args = 
	{
		L"-E", L"main",         // Entry point
		L"-T", type_arg,		// Target profile
		L"-Zi",                 // Debug info
		L"-Qembed_debug",       // Embed debug info in shader
		L"-Od"
	};
	
	dxc_compiler->Compile(&source_buffer, Args.GetData(), Args.Num(), dxc_include_header, IID_PPV_ARGS(&compile_result));

	HRESULT compile_status;

	bool compile_succeded = SUCCEEDED(compile_result->GetStatus(&compile_status)) && SUCCEEDED(compile_status);

	if(compile_succeded == false)
	{
		TComPtr< IDxcBlobEncoding> error_blob;

		compile_result->GetErrorBuffer(&error_blob);

		char* error_message = new char[error_blob->GetBufferSize() + 1];

		Memory::MemCopy(error_message, error_blob->GetBufferSize(), error_blob->GetBufferPointer(), error_blob->GetBufferSize());

		GAssert(false);

		delete[] error_message;

		return false;
	}

	GAssert(compile_result->HasOutput(DXC_OUT_OBJECT));

	TComPtr<IDxcBlob> dxil_blob;

	compile_result->GetOutput(DXC_OUT_OBJECT, IID_PPV_ARGS(&dxil_blob), nullptr);

	mDxil.SetNum(dxil_blob->GetBufferSize());

	Memory::MemCopy(mDxil.GetData(), mDxil.Num(), dxil_blob->GetBufferPointer(), dxil_blob->GetBufferSize());

	TComPtr<IDxcBlob> reflection_blob;

	compile_result->GetOutput(DXC_OUT_REFLECTION, IID_PPV_ARGS(&reflection_blob), nullptr);

	DxcBuffer reflection_buffer;

	reflection_buffer.Ptr = reflection_blob->GetBufferPointer();
	reflection_buffer.Size = reflection_blob->GetBufferSize();
	reflection_buffer.Encoding = DXC_CP_ACP;

	dxil_utils->CreateReflection(&reflection_buffer, IID_PPV_ARGS(&mReflection));

	D3D12_SHADER_DESC shader_desc;

	mReflection->GetDesc(&shader_desc);

	mNumSignatureParameters = shader_desc.InputParameters;
	mNumBoundResources = shader_desc.BoundResources;

	return true;
}

void Shader::BuildRootSignature(TArray<D3D12_ROOT_PARAMETER1>& OutRootSignatureDescription)
{
	D3D12_SHADER_DESC shader_desc;

	mReflection->GetDesc(&shader_desc);

	for (int32 i = 0; i < shader_desc.BoundResources; i++)
	{
		D3D12_SHADER_INPUT_BIND_DESC bound_desc;

		mReflection->GetResourceBindingDesc(i, &bound_desc);

		D3D12_ROOT_PARAMETER1 root_parameter = {};

		root_parameter.ShaderVisibility = mType == EShaderType::Vertex ? D3D12_SHADER_VISIBILITY_VERTEX : D3D12_SHADER_VISIBILITY_PIXEL;

		if (bound_desc.Type == D3D_SIT_TEXTURE)
		{
			D3D12_DESCRIPTOR_RANGE1 texture_desc_range;

			texture_desc_range.BaseShaderRegister = bound_desc.BindPoint;
			texture_desc_range.RegisterSpace = bound_desc.Space;
			texture_desc_range.RangeType = D3D12_DESCRIPTOR_RANGE_TYPE_SRV;
			texture_desc_range.NumDescriptors = 1;
			texture_desc_range.Flags = D3D12_DESCRIPTOR_RANGE_FLAG_DATA_STATIC_WHILE_SET_AT_EXECUTE;
			texture_desc_range.OffsetInDescriptorsFromTableStart = 0;

			mDescriptorRanges.Add(texture_desc_range);

			root_parameter.ParameterType = D3D12_ROOT_PARAMETER_TYPE_DESCRIPTOR_TABLE;
			root_parameter.DescriptorTable.NumDescriptorRanges = 1;
			root_parameter.DescriptorTable.pDescriptorRanges = &mDescriptorRanges.Last();
		}
		else if (bound_desc.Type == D3D_SIT_SAMPLER)
		{
			continue;
			//D3D12_DESCRIPTOR_RANGE1 ps_sampler_desc_range;

			//ps_sampler_desc_range.BaseShaderRegister = bound_desc.BindPoint;
			//ps_sampler_desc_range.RegisterSpace = bound_desc.Space;
			//ps_sampler_desc_range.RangeType = D3D12_DESCRIPTOR_RANGE_TYPE_SAMPLER;
			//ps_sampler_desc_range.NumDescriptors = 1;
			//ps_sampler_desc_range.Flags = D3D12_DESCRIPTOR_RANGE_FLAG_NONE;
			//ps_sampler_desc_range.OffsetInDescriptorsFromTableStart = 0;

			//mDescriptorRanges.Add(ps_sampler_desc_range);

			//root_parameter.ParameterType = D3D12_ROOT_PARAMETER_TYPE_DESCRIPTOR_TABLE;
			//root_parameter.DescriptorTable.NumDescriptorRanges = 1;
			//root_parameter.DescriptorTable.pDescriptorRanges = &mDescriptorRanges.Last();

		}
		else if (bound_desc.Type == D3D_SIT_CBUFFER)
		{
			ID3D12ShaderReflectionConstantBuffer* constant_buffer = mReflection->GetConstantBufferByName(bound_desc.Name);

			D3D12_SHADER_BUFFER_DESC constant_buffer_desc;

			constant_buffer->GetDesc(&constant_buffer_desc);

			D3D12_DESCRIPTOR_RANGE1 const_buffer_desc_range = {};

			const_buffer_desc_range.BaseShaderRegister = bound_desc.BindPoint;
			const_buffer_desc_range.RegisterSpace = bound_desc.Space;
			const_buffer_desc_range.RangeType = D3D12_DESCRIPTOR_RANGE_TYPE_CBV;
			const_buffer_desc_range.NumDescriptors = 1;
			const_buffer_desc_range.Flags = D3D12_DESCRIPTOR_RANGE_FLAG_DATA_STATIC_WHILE_SET_AT_EXECUTE;

			mDescriptorRanges.Add(const_buffer_desc_range);

			root_parameter.ParameterType = D3D12_ROOT_PARAMETER_TYPE_32BIT_CONSTANTS;
			root_parameter.Constants.Num32BitValues = constant_buffer_desc.Size / 4;
			root_parameter.Constants.ShaderRegister = bound_desc.BindPoint;
			root_parameter.Constants.RegisterSpace = bound_desc.Space;
		}
		else
		{
			GAssert(false);
		}

		mRootParameters.Add(root_parameter);
		OutRootSignatureDescription.Add(root_parameter);
	}
}
