#include "Memory/Memory.h"

#include <memory>

#include "Memory/MemoryAllocator.h"

class MemoryInitialisation
{
public:

	MemoryInitialisation()
	{
		MemoryAllocator::Get().Initialise();
	}

	~MemoryInitialisation()
	{

	}
};

#pragma init_seg(lib)
MemoryInitialisation MemoryInit;

void Memory::MemZero(void* Data, uint32 SizeInBytes)
{
	memset(Data, 0, SizeInBytes);
}

void Memory::MemCopy(void* Destination, uint32 DestinationSize, void* Source, uint32 SourceSize)
{
	memcpy_s(Destination, DestinationSize, Source, SourceSize);
}

void Memory::MemSet(void* Destination, uint32 DestinationSize, uint64 Value)
{
	memset(Destination, Value, DestinationSize);
}
