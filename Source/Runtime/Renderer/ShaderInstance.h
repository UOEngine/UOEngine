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

	//uint32						GetNumSrvs(EShaderType inShaderType) const									{return mBindingInfo[static_cast<uint32>(inShaderType)].mNumSrvs; }

	//int32						GetSrvRootSignatureIndex(EShaderType inShaderType, uint32 inIndex) const;

	ShaderBindingHandle			GetParameter(EShaderType inShaderType, const char* inName) const;

	void						SetTexture(ShaderBindingHandle inBindingHandle, RenderTexture* inTexture);
	void						SetBuffer(ShaderBindingHandle inBindingHandle, RenderBuffer* inBuffer);

	const ShaderBoundData*		GetBoundData(EShaderType inShaderType) const								{return &mBoundData[static_cast<uint32>(inShaderType)];}

	Shader*						GetShader(EShaderType inShaderType)	const									{return GetShader(static_cast<uint32>(inShaderType));}
	Shader*						GetShader(uint32 inIndex) const												{return mShaderPrograms[inIndex]; }
private:


	static constexpr uint32		sNumShaderTypes = static_cast<uint32>(EShaderType::Count);
	//static constexpr uint32		sMaxBoundTextures = 4;
	//static constexpr uint32		sMaxBoundBuffers = 4;

	Shader*						mShaderPrograms[sNumShaderTypes];

	//RenderTexture*				mTextures[sNumShaderTypes][sMaxBoundTextures];
	//RenderBuffer*				mBuffers[sNumShaderTypes][sMaxBoundBuffers];

	//SrvBindingInfo				mBindingInfo[sNumShaderTypes];

	ShaderBoundData				mBoundData[sNumShaderTypes];
};