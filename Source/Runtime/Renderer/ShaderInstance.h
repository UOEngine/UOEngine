#pragma once

#include "Renderer/Shader.h"

class RenderBuffer;
class RenderTexture;

struct Slot
{
	union
	{
		RenderTexture*	mTexture;
		RenderBuffer*	mBuffer;
	};
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

	const ShaderBoundData*		GetBoundData(EShaderType inShaderType) const								{return &mBoundData[static_cast<uint32>(inShaderType)];}

	Shader*						GetShader(EShaderType inShaderType)	const									{return GetShader(static_cast<uint32>(inShaderType));}
	Shader*						GetShader(uint32 inIndex) const												{return mShaderPrograms[inIndex]; }

private:


	static constexpr uint32		sNumShaderTypes = static_cast<uint32>(EShaderType::Count);

	Shader*						mShaderPrograms[sNumShaderTypes];

	ShaderBoundData				mBoundData[sNumShaderTypes];
};