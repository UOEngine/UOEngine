#pragma once

#include "Core/Types.h"

class Allocator
{
					Allocator();
public:
	static void*	Allocate(uint32 NumBytes);

	static void		Free(void* Address);

	static void*	Reallocate(void* Address, uint32 BytesNewSize);

	static void		Memzero(void *Address, uint32 SizeInBytes);

	static void		Memcpy(void* CopyFromAddress, uint32 SizeInBytesOfCopy, void* CopyToAddress);
};
