#pragma once

#include "Core/Types.h"

class RenderBuffer;

extern "C"
{
	__declspec(dllexport) RenderBuffer*	CreateRenderBuffer(uint32 inNumElements, uint32 inStride);

	__declspec(dllexport) void	SetData(RenderBuffer* inBuffer, void* inData, uint32 inNumElements);

}