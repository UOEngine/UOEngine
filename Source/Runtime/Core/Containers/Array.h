#pragma once

#include <initializer_list>

#include "Core/Assert.h"
#include "Core/Types.h"

template <typename DataType>
class TArray
{
public:
							TArray();
							
							TArray(std::initializer_list<DataType> InitList);

							TArray(const DataType* Ptr, uint32 NumToCopy);

							~TArray();

	DataType&				operator[](int32 Index);

	int32					Add(const DataType& NewElement);

	uint32					GetCapacity() const					{ return Capacity;}

	bool					Reserve(int32 MinNumElements);

	void					SetNum(uint32 NewSize);

	uint32					Num() const;

	DataType*				GetData()							{return Data;}

	void					Copy(const DataType* OtherData, uint32 OtherNumElements);

private:

	DataType*				Data = nullptr;
	int32					NumElements = 0;
	uint32					Capacity = 0;
};

#include "Core/Containers/Array.inl"