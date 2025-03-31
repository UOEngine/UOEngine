#pragma once

#include "Core/Assert.h"
#include "Core/Types.h"

class Allocator;

template <typename DataType, class AllocatorType=Allocator>
class TArray: public TFixedArray<DataType>
{
							TArray();

	DataType				operator[](int32 Index) {return Data[Index]; }

	int32					Add(int32 Index);

	int32					Capacity() const;

	bool					Reserve(int32 MinNumElements);

	void					Resize(int32 NewSizeBytes);

private:

	DataType*				Memory;
	int32					NumElements;

	TFixedArray<DataType>	Data;
};

#include "Core/Containers/Array.inl"