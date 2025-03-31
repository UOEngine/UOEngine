#pragma once

#include "Core/Types.h"

class Allocator
{
	static void*	Allocate(int32 NumBytes);

	static void		Free(void* Address);

	static void*	Reallocate(void* Address, int32 BytesNewSize);
};
