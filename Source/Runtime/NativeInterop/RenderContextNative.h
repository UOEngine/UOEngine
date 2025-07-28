#pragma once

class RenderTexture;
class ShaderInstance;

extern "C"
{
	__declspec(dllexport) void				SetShaderBindingData(RenderTexture* inTexture);

	__declspec(dllexport) ShaderInstance*	GetShaderInstance();

	__declspec(dllexport) void				Draw();

}