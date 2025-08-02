#pragma once

#include "Core/Types.h"

class RenderTexture;
class ShaderInstance;

extern "C"
{
	__declspec(dllexport) void				SetShaderBindingData(RenderTexture* inTexture);

	__declspec(dllexport) ShaderInstance*	GetShaderInstance();

	__declspec(dllexport) void				SetBindlessTextures(RenderTexture** inTextures, uint32 inNum);

	__declspec(dllexport) void				Draw(uint32 inNumInstances);

}