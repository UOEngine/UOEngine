#include "NativeInterop/ShaderInstanceNative.h"

#include "D3D12RHI/ShaderInstance.h"

void SetTexture(ShaderInstance* inShaderInstance, const char* inParameterName, RenderTexture* inTexture)
{
	inShaderInstance->SetTexture(inParameterName, inTexture);
}

void SetBuffer(ShaderInstance* inShaderInstance, const char* inParameterName, RenderBuffer* inBuffer)
{
	inShaderInstance->SetBuffer(inParameterName, inBuffer);

}

void SetMatrix(ShaderInstance* inShaderInstance, const char* inParameterName, float* inMatrix)
{
	Matrix4x4 matrix;

	for (int32 i = 0; i < Matrix4x4::NumElements; i++)
	{
		matrix[i] = inMatrix[i];
	}

	inShaderInstance->SetVariable(inParameterName, matrix);
}

void SetVariable(ShaderInstance* inShaderInstance, const char* inParameterName, void* inVariable, uint32 inSize)
{
	inShaderInstance->SetVariable(inParameterName, inVariable, inSize);

}
