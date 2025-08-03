#include "ShaderInstance.h"

#include "RenderTexture.h"

void ShaderInstance::Init(Shader* inVertexProgram, Shader* inPixelProgram)
{
	const uint32 vertex_program_index = static_cast<uint32>(EShaderType::Vertex);
	const uint32 pixel_program_index = static_cast<uint32>(EShaderType::Pixel);

	mShaderPrograms[vertex_program_index] = inVertexProgram;
	mShaderPrograms[pixel_program_index] = inPixelProgram;

	mBoundData[vertex_program_index].mData.SetNum(inVertexProgram->GetBindingInfo()->mBindings.Num());
	mBoundData[pixel_program_index].mData.SetNum(inPixelProgram->GetBindingInfo()->mBindings.Num());

}

//int32 ShaderInstance::GetSrvRootSignatureIndex(EShaderType inShaderType, uint32 inIndex) const
//{
//	uint32 shader_program_index = static_cast<uint32>(inShaderType);
//
//	GAssert(inIndex < SrvBindingInfo::sMaxRootIndices);
//
//	int32 root_signature_index = mBindingInfo[shader_program_index].mRootIndices[inIndex];
//
//	return root_signature_index;
//}

ShaderBindingHandle ShaderInstance::GetParameter(EShaderType inShaderType, const char* inName) const
{
	Shader* shader = mShaderPrograms[static_cast<uint32>(inShaderType)];

	ShaderBindingHandle handle = shader->GetParameter(inName);

	return handle;
}

void ShaderInstance::SetTexture(ShaderBindingHandle inBindingHandle, RenderTexture* inTexture)
{
	GAssert(inBindingHandle.IsValid());

	mBoundData[inBindingHandle.mShaderType].mData[inBindingHandle.mHandle].mTexture = inTexture;
}

void ShaderInstance::SetTexture(const char* inName, RenderTexture* inTexture)
{
	for (int i = 0; i < sNumShaderTypes; i++)
	{
		ShaderBindingHandle handle = mShaderPrograms[i]->GetParameter(inName);

		if (handle.IsValid())
		{
			SetTexture(handle, inTexture);

			return;
		}
	}

	GUnreachable;
}

void ShaderInstance::SetBuffer(ShaderBindingHandle inBindingHandle, RenderBuffer* inBuffer)
{
	GAssert(inBindingHandle.IsValid());

	mBoundData[inBindingHandle.mShaderType].mData[inBindingHandle.mHandle].mBuffer = inBuffer;
}

void ShaderInstance::SetBuffer(const char* inName, RenderBuffer* inBuffer)
{
	bool found = false;

	for (int i = 0; i < sNumShaderTypes; i++)
	{
		if (mShaderPrograms[i] == nullptr)
		{
			continue;
		}

		ShaderBindingHandle handle = mShaderPrograms[i]->GetParameter(inName);

		if (handle.IsValid())
		{
			SetBuffer(handle, inBuffer);

			found = true;
		}
	}

	if (found)
	{
		return;
	}

	GUnreachable;
}

