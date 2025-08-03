#pragma once

#include "Core/Types.h"

class MemoryAllocator
{
public:

								MemoryAllocator();

	static MemoryAllocator&		Get();

	void						Initialise();

	void*						Allocate(uint64 SizeInBytes);
	void*						Reallocate(void* Data, uint64 NewSizeInBytes);
	void						Free(void* Data);

	uint64						GetTotalAllocationSizeBytes() const {return mTotalAllocationSizeBytes;}

private:

	static void					ErrorHandler(int32 Error, void* Arg);
	static void					OutputHandler(const char* Message, void* Arg);

	uint64						mTotalAllocationSizeBytes;
	uint64						mMaxAllowedAllocationBytes;

};
