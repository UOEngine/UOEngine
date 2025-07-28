#include "NativeInterop/ShaderInstanceNative.h"

#include "Renderer/ShaderInstance.h"

void SetTexture(ShaderInstance* inShaderInstance, const char* inParameterName, RenderTexture* inTexture)
{
	inShaderInstance->SetTexture(inParameterName, inTexture);
}

void SetBuffer(ShaderInstance* inShaderInstance, const char* inParameterName, RenderBuffer* inBuffer)
{
	inShaderInstance->SetBuffer(inParameterName, inBuffer);

}
