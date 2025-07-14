#pragma once

#include "Core/Types.h"

class RenderTexture;

extern "C"
{
	__declspec(dllexport) RenderTexture*	CreateTexture(uint32 Width, uint32 Height);

	__declspec(dllexport) void				SetTextureData(RenderTexture* inTexture, uint8* inData, uint32 inSize);

}//