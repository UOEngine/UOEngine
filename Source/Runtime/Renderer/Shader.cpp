#include "Renderer/Shader.h"

#include <wrl/client.h>

#include "Core/FileReader.h"
#include <dxcapi.h>

using namespace Microsoft::WRL;

Shader::Shader(EShaderType InType)
{
	Type = InType;
}

void Shader::Load(const String& FilePath)
{
	FileHandle* shader_file = nullptr;

	if (FileDevice::Open(FilePath, shader_file) == false)
	{
		GAssert(false);

		return;
	}
	
	TArray<uint8> shader_contents;

	shader_contents.SetNum(shader_file->GetSize());

	FileDevice::Read(shader_file, shader_contents.GetData());

	ComPtr<IDxcUtils>			dxil_utils;
	ComPtr<IDxcCompiler3>		dxc_compiler;
	ComPtr<IDxcIncludeHandler>	dxc_include_header;

	// Initialize DXC components
	DxcCreateInstance(CLSID_DxcUtils, IID_PPV_ARGS(&dxil_utils));
	DxcCreateInstance(CLSID_DxcCompiler, IID_PPV_ARGS(&dxc_compiler));

	dxil_utils->CreateDefaultIncludeHandler(&dxc_include_header);

	ComPtr<IDxcBlobEncoding> dxc_source_blob;

	HRESULT create_blob_result = dxil_utils->CreateBlobFromPinned(shader_contents.GetData(), shader_contents.Num(), DXC_CP_UTF8, &dxc_source_blob);

	DxcBuffer source_buffer;

	source_buffer.Ptr = dxc_source_blob->GetBufferPointer();
	source_buffer.Size = dxc_source_blob->GetBufferSize();
	source_buffer.Encoding = DXC_CP_UTF8;

	ComPtr<IDxcResult> compile_result;

	TArray<LPCWSTR> Args = 
	{
		L"-E", L"main",           // Entry point
		L"-T", L"vs_6_0",         // Target profile
		L"-Zi",                   // Debug info
		L"-Qembed_debug",         // Embed debug info in shader
	};
	
	dxc_compiler->Compile(&source_buffer, Args.GetData(), Args.Num(), dxc_include_header.Get(), IID_PPV_ARGS(&compile_result));

	HRESULT CompileStatus;

	bool compile_succeded = SUCCEEDED(compile_result->GetStatus(&CompileStatus)) && SUCCEEDED(CompileStatus);

	if(compile_succeded == false)
	{
		ComPtr< IDxcBlobEncoding> error_blob;

		compile_result->GetErrorBuffer(&error_blob);

		char* error_message = new char[error_blob->GetBufferSize() + 1];

		Memory::MemCopy(error_message, error_blob->GetBufferSize(), error_blob->GetBufferPointer(), error_blob->GetBufferSize());

		GAssert(false);

		delete[] error_message;

		return;
	}

	GAssert(compile_result->HasOutput(DXC_OUT_OBJECT));

	ComPtr<IDxcBlob> dxil_blob;

	compile_result->GetOutput(DXC_OUT_OBJECT, IID_PPV_ARGS(&dxil_blob), nullptr);

	Dxil.SetNum(dxil_blob->GetBufferSize());

	Memory::MemCopy(Dxil.GetData(), Dxil.Num(), dxil_blob->GetBufferPointer(), dxil_blob->GetBufferSize());

}
