#pragma once

#include "Core/Assert.h"
#include "Core/Types.h"

template <typename DataType>
class TArray
{
public:
							TArray();

							TArray(uint32 InitialCapacity);

							~TArray();

	DataType&				operator[](int32 Index);

	int32					Add(const DataType& NewElement);

	uint32					GetCapacity() const					{ return Capacity;}

	bool					Reserve(int32 MinNumElements);

	void					SetNum(uint32 NewSize);

	uint32					Num() const;

	DataType*				GetData()							{return Data;}

private:

	DataType*				Data;
	int32					NumElements;
	uint32					Capacity;
};

#include "Core/Containers/Array.inl"