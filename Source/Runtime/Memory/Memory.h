#pragma once

#include "Core/Types.h"

struct Memory
{
	static void MemZero(void* Data, uint32 SizeInBytes);

	static void MemCopy(void* Destination, uint32 DestinationSize, void* Source, uint32 SourceSize);

	static void MemSet(void* Destination, uint32 DestinationSize, uint64 Value);
};