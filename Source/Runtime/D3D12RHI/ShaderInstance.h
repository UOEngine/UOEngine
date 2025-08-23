#pragma once

#include "Core/Math/Matrix4x4.h"
#include "Shader.h"

class RenderBuffer;
class RenderTexture;

struct Slot
{
	ShaderBinding mBindingInfo;
	TArray<uint8> mData;
};

struct ShaderBoundData
{
	TArray<Slot> mData;
};

class ShaderInstance
{
public:

	void						Init(Shader* inVertexProgram, Shader* inPixelProgram);

	ShaderBindingHandle			GetParameter(EShaderType inShaderType, const char* inName) const;

	void						SetTexture(ShaderBindingHandle inBindingHandle, RenderTexture* inTexture);
	void						SetTexture(const char* inName, RenderTexture* inTexture);

	void						SetBuffer(ShaderBindingHandle inBindingHandle, RenderBuffer* inBuffer);
	void						SetBuffer(const char* inName, RenderBuffer* inBuffer);

	void						SetVariable(const char* inName, const Matrix4x4& inMatrix);
	void						SetVariable(ShaderBindingHandle inBindingHandle, const Matrix4x4& inMatrix);

	const ShaderBoundData*		GetBoundData(EShaderType inShaderType) const								{return &mBoundData[static_cast<uint32>(inShaderType)];}

	Shader*						GetShader(EShaderType inShaderType)	const									{return GetShader(static_cast<uint32>(inShaderType));}
	Shader*						GetShader(uint32 inIndex) const												{return mShaderPrograms[inIndex]; }

private:

	static constexpr uint32		sNumShaderTypes = static_cast<uint32>(EShaderType::Count);

	Shader*						mShaderPrograms[sNumShaderTypes];

	ShaderBoundData				mBoundData[sNumShaderTypes];
};