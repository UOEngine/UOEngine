#include "Memory/MemoryAllocator.h"

#include "mimalloc.h"

#include "Core/Assert.h"

MemoryAllocator& MemoryAllocator::Get()
{
	static MemoryAllocator Instance;

	return Instance;
}

void MemoryAllocator::Initialise()
{
	mi_register_error(&MemoryAllocator::ErrorHandler, nullptr);
	mi_register_output(&MemoryAllocator::OutputHandler, nullptr);
}

void* MemoryAllocator::Allocate(uint64 SizeInBytes)
{
	void* Data = mi_malloc(SizeInBytes);

	TotalAllocationSizeBytes += SizeInBytes;

	return Data;
}

void* MemoryAllocator::Reallocate(void* Data, uint64 NewSizeInBytes)
{
	uint64 OldSizeOfDataBytes = mi_usable_size(Data);

	TotalAllocationSizeBytes -= OldSizeOfDataBytes;

	void* NewData = mi_realloc(Data, NewSizeInBytes);

	TotalAllocationSizeBytes += NewSizeInBytes;

	return NewData;
}

void MemoryAllocator::Free(void* Data)
{
	uint64 SizeOfDataBytes = mi_usable_size(Data);

	mi_free(Data);

	TotalAllocationSizeBytes -= SizeOfDataBytes;
}

void MemoryAllocator::ErrorHandler(int32 Error, void* Arg)
{
	GAssert(false);
}

void MemoryAllocator::OutputHandler(const char* Message, void* Arg)
{

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