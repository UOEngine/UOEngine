#pragma once

class RenderBuffer;
class RenderTexture;
class ShaderInstance;

extern "C"
{
	__declspec(dllexport) void SetTexture(ShaderInstance* inShaderInstance, const char* inParameterName, RenderTexture* inTexture);

	__declspec(dllexport) void SetBuffer(ShaderInstance* inShaderInstance, const char* inParameterName, RenderBuffer* inBuffer);

	__declspec(dllexport) void SetMatrix(ShaderInstance* inShaderInstance, const char* inParameterName, float* inMatrix);

}