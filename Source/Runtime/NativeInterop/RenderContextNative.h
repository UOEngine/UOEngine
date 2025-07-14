#pragma once

class RenderTexture;

extern "C"
{
	__declspec(dllexport) void	SetShaderBindingData(RenderTexture* inTexture);

	__declspec(dllexport) void	Draw();

}