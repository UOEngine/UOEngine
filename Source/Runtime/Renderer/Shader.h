#pragma once

#include <d3d12.h>
#include <d3d12shader.h>

#include "Core/Containers/String.h"
#include "Core/TComPtr.h"

struct ID3D12ShaderReflection;

enum class EShaderType : uint8
{
	Vertex,
	Pixel,
	Compute,
	Invalid
};

//struct ShaderBoundResource
//{
//	String						mName;
//	uint32						mBindPoint;
//	D3D12_ROOT_PARAMETER_TYPE	mType;
//};

class Shader
{
public:

										Shader(EShaderType InType);

	bool								Load(const String& FilePath);

	uint8*								GetBytecode()					const {return mDxil.GetData();}
	uint32								GetBytecodeLength()				const {return mDxil.Num();}

	uint32								GetNumSignatureParameters()		const { return mNumSignatureParameters; }
	uint32								GetNumBoundResources()			const { return mNumSignatureParameters; }
	//D3D12_ROOT_PARAMETER1				GetRootParameter(uint32 i)		const {return mRootParameters[i]; }
	//TComPtr<ID3D12ShaderReflection>		GetReflection()			const {return mReflection;}

	void								BuildRootSignature(TArray<D3D12_ROOT_PARAMETER1>& OutRootSignatureDescription);

private:

	EShaderType							mType;

	TArray<uint8>						mDxil;

	uint32								mNumSignatureParameters;

	TArray<D3D12_ROOT_PARAMETER1>		mRootParameters;
	uint32								mNumBoundResources;

	TArray<D3D12_DESCRIPTOR_RANGE1>		mDescriptorRanges;

	TComPtr<ID3D12ShaderReflection>		mReflection;

};