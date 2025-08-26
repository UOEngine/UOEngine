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

	for (int32 i = 0; i < inVertexProgram->GetBindingInfo()->mBindings.Num(); i++)
	{
		Slot& slot = mBoundData[vertex_program_index].mData[i];
		const ShaderBinding& binding_info = inVertexProgram->GetBindingInfo()->mBindings[i];

		slot.mBindingInfo = binding_info;
		slot.mData.SetNum(binding_info.mSizeBytes);
	}

	for (int32 i = 0; i < inPixelProgram->GetBindingInfo()->mBindings.Num(); i++)
	{
		Slot& slot = mBoundData[pixel_program_index].mData[i];
		const ShaderBinding& binding_info = inPixelProgram->GetBindingInfo()->mBindings[i];

		slot.mBindingInfo = binding_info;
		slot.mData.SetNum(binding_info.mSizeBytes);
	}

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

ShaderBindingHandle ShaderInstance::GetParameter(const char* inName) const
{
	for (int i = 0; i < sNumShaderTypes; i++)
	{
		if (mShaderPrograms[i] == nullptr)
		{
			continue;
		}

		ShaderBindingHandle handle = mShaderPrograms[i]->GetParameter(inName);

		if (handle.IsValid())
		{
			return handle;
		}
	}

	GUnreachable;

	return ShaderBindingHandle{};
}

void ShaderInstance::SetTexture(ShaderBindingHandle inBindingHandle, RenderTexture* inTexture)
{
	GAssert(inBindingHandle.IsValid());

	RenderTexture* buffer[] = {inTexture};

	mBoundData[inBindingHandle.mShaderType].mData[inBindingHandle.mHandle].mData.Copy((uint8*)buffer, sizeof(RenderTexture*));
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

	RenderBuffer* buffer[] = {inBuffer};

	mBoundData[inBindingHandle.mShaderType].mData[inBindingHandle.mHandle].mData.Copy((uint8*)buffer, sizeof(RenderBuffer*));

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

void ShaderInstance::SetVariable(const char* inName, const Matrix4x4& inMatrix)
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
			SetVariable(handle, inMatrix);

			found = true;
		}
	}

	if (found)
	{
		return;
	}

	GUnreachable;
}

void ShaderInstance::SetVariable(ShaderBindingHandle inBindingHandle, const Matrix4x4& inMatrix)
{
	GAssert(inBindingHandle.IsValid());

	mBoundData[inBindingHandle.mShaderType].Copy(inBindingHandle.mHandle, (void*)&inMatrix, sizeof(Matrix4x4));
}

void ShaderInstance::SetVariable(const char* inName, void* inVariable, uint32 inSize)
{
	ShaderBindingHandle binding_handle = GetParameter(inName);

	mBoundData[binding_handle.mShaderType].Copy(binding_handle.mHandle, inVariable, inSize);
}
