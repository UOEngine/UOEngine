#pragma once

#include "Core/Types.h"

struct IntVector2DNative
{
	int32 X;
	int32 Y;
};

extern "C"
{
	__declspec(dllexport) IntVector2DNative GetExtents();
}
