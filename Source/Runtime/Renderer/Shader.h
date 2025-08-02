#pragma once

#include <d3d12.h>
#include <d3d12shader.h>

#include "Core/Containers/String.h"
#include "Core/TComPtr.h"

struct ID3D12ShaderReflection;

enum class EShaderType : uint8
{
	Vertex = 0,
	Pixel,
	Compute,
	Count,
	Invalid
};

enum class EShaderUpdateFrequency
{
	PerFrame = 0, // Space0 etc.
	PerDraw,
	Bindless,
	Count,
	Invalid
};

enum class EShaderBindingType : uint8
{
	ConstantBuffer = 0,
	Texture,
	StructuredBuffer,
	Sampler,
	BindlessTexture,
	Count,
	Invalid
};

struct ShaderBindingHandle
{
	uint32					mHandle = sInvalidHandle;
	uint8					mShaderType = 0;

	static const uint32		sInvalidHandle = 0xFFFFFFFF;

	bool IsValid() const	{return mHandle != sInvalidHandle;}
};

struct ShaderBinding
{
	String					mName;
	uint32					mBindIndex;
	uint8					mRootParameterIndex;
	EShaderBindingType		mType = EShaderBindingType::Invalid;
	EShaderUpdateFrequency	mUpdateFrequency = EShaderUpdateFrequency::Invalid;
};

struct ShaderBindingInfo
{
	TArray<ShaderBinding>	mBindings;
};

class Shader
{
public:

										Shader(EShaderType InType);

	bool								Load(const String& FilePath);

	uint8*								GetBytecode() const						{return mDxil.GetData();}
	uint32								GetBytecodeLength() const				{return mDxil.Num();}

	uint32								GetNumSignatureParameters()	const		{ return mNumSignatureParameters; }
	uint32								GetNumBoundResources() const			{ return mNumSignatureParameters; }

	void								BuildRootSignature(TArray<D3D12_ROOT_PARAMETER1>& OutRootSignatureDescription);

	//int32								GetRootSignatureBindIndex(uint32 inIndex) const;

	ShaderBindingHandle					GetParameter(const char* inName);

	ShaderBindingHandle					GetParameter(uint32 inIndex);

	const ShaderBindingInfo*			GetBindingInfo() const					{return &mBindingInfo;}

private:

	ShaderBindingInfo					mBindingInfo;

	EShaderType							mType;

	TArray<uint8>						mDxil;

	uint32								mNumSignatureParameters;

	//TArray<D3D12_ROOT_PARAMETER1>		mRootParameters;
	uint32								mNumBoundResources;

	TArray<D3D12_DESCRIPTOR_RANGE1>		mDescriptorRanges;

	TComPtr<ID3D12ShaderReflection>		mReflection;

	//TArray<ShaderBinding>				mShaderBindings;

	//uint32								mNumSrvs;

};