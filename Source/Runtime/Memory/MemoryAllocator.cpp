#include "Memory/MemoryAllocator.h"

#include "mimalloc.h"

#include "Core/Assert.h"

MemoryAllocator::MemoryAllocator()
{
	mTotalAllocationSizeBytes = 0;
	mMaxAllowedAllocationBytes = 0;
}

MemoryAllocator& MemoryAllocator::Get()
{
	static MemoryAllocator Instance;

	return Instance;
}

void MemoryAllocator::Initialise()
{
	mi_option_enable(mi_option_verbose);      // log all allocations
	mi_option_enable(mi_option_show_errors);

	mi_register_error(&MemoryAllocator::ErrorHandler, nullptr);
	mi_register_output(&MemoryAllocator::OutputHandler, nullptr);

	const uint64 one_mb = 1024 * 1024;

	mMaxAllowedAllocationBytes = 1024 * one_mb; // 1 gig.
}

void* MemoryAllocator::Allocate(uint64 SizeInBytes)
{
	if (mTotalAllocationSizeBytes + SizeInBytes > mMaxAllowedAllocationBytes)
	{
		GAssert(false); // Hit limit.
	}

	void* Data = mi_malloc(SizeInBytes);

	mTotalAllocationSizeBytes += SizeInBytes;

	return Data;
}

void* MemoryAllocator::Reallocate(void* Data, uint64 NewSizeInBytes)
{
	uint64 OldSizeOfDataBytes = mi_usable_size(Data);

	mTotalAllocationSizeBytes -= OldSizeOfDataBytes;

	void* NewData = mi_realloc(Data, NewSizeInBytes);

	mTotalAllocationSizeBytes += NewSizeInBytes;

	return NewData;
}

void MemoryAllocator::Free(void* Data)
{
	uint64 SizeOfDataBytes = mi_usable_size(Data);

	mi_free(Data);

	mTotalAllocationSizeBytes -= SizeOfDataBytes;
}

void MemoryAllocator::ErrorHandler(int32 Error, void* Arg)
{
	GAssert(false);
}

void MemoryAllocator::OutputHandler(const char* Message, void* Arg)
{
	printf(Message);
}

void* operator new(uint64 SizeInBytes)
{
	return MemoryAllocator::Get().Allocate(SizeInBytes);
}

void operator delete(void* Pointer)
{
	MemoryAllocator::Get().Free(Pointer);
}

void* operator new[](uint64 SizeInBytes)
{
	return MemoryAllocator::Get().Allocate(SizeInBytes);
}

void operator delete[](void* Pointer)
{
	MemoryAllocator::Get().Free(Pointer);
}