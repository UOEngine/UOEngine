#pragma once

#include "Core/Types.h"

extern "C"
{
	__declspec(dllexport) int32 EngineInit();
	__declspec(dllexport) int32 EnginePreUpdate();
	__declspec(dllexport) void EnginePostUpdate();
	__declspec(dllexport) void EngineShutdown();
}