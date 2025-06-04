#include "Memory/Allocator.h"

#include "Core/Assert.h"

void* Allocator::Allocate(uint32 NumBytes)
{
	uint8* Data = new uint8[NumBytes];

	Memzero(Data, NumBytes);

	return Data;
}

void Allocator::Free(void* Address)
{
	delete[] Address;
}

void* Allocator::Reallocate(void* Address, uint32 BytesNewSize)
{
	GAssert(false);

	return nullptr;
}

void Allocator::Memzero(void* Address, uint32 SizeInBytes)
{
	uint8* Start = static_cast<uint8*>(Address);

	for (int32 i = 0; i < SizeInBytes; i++)
	{
		Start[i] = 0;
	}
}

void Allocator::Memcpy(void* CopyFromAddress, uint32 SizeInBytesOfCopy, void* CopyToAddress)
{
	uint8* Start = static_cast<uint8*>(CopyFromAddress);
	uint8* Destination = static_cast<uint8*>(CopyToAddress);

	for (int32 i = 0; i < SizeInBytesOfCopy; i++)
	{
		Destination[i] = Start[i];
	}
}

