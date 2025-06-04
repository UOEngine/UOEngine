#pragma once

#include "Core/Assert.h"
#include "Core/Types.h"

class Allocator;

template <typename DataType, class AllocatorType=Allocator>
class TArray
{
public:
							TArray();

							TArray(uint32 InitialCapacity);

	DataType&				operator[](int32 Index);

	int32					Add(const DataType& NewElement);

	uint32					GetCapacity() const { return Capacity;}

	bool					Reserve(int32 MinNumElements);

	void					Resize(uint32 NewSize);

	void					SetNum(uint32 NewSize);

	uint32					Num() const;

private:

	DataType*				Data;
	int32					NumElements;
	uint32					Capacity;
};

#include "Core/Containers/Array.inl"