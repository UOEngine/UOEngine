#pragma once

#include <initializer_list>

#include "Core/Assert.h"
#include "Core/Containers/Span.h"
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
	const DataType&			operator[](int32 Index) const;

	int32					Add(const DataType& NewElement);
	DataType&				AddEmpty();

	uint32					GetCapacity() const					{ return Capacity;}

	bool					Reserve(int32 MinNumElements);

	void					SetNum(uint32 NewSize);

	uint32					Num() const;

	DataType*				GetData()							{return Data;}
	DataType*				GetData() const						{return Data;}

	DataType&				Last()								{return Data[NumElements - 1]; }

	void					Copy(const DataType* OtherData, uint32 OtherNumElements);
	
	template<class T>
	TSpan<T>				AsSpan()
							{
								uint64 size_of_new_type = sizeof(T);
								uint64 size_of_old_type = sizeof(DataType);
								uint64 conversion = size_of_old_type / size_of_new_type;

								GAssert(conversion > 0); // Fix later when new type > old type.

								uint64 new_size = conversion * NumElements;

								return TSpan<T>((T*)Data, new_size);
							}

	void					RemoveAt(int32 Index, bool inbShrink);

	void					Move(int32 inIndexFrom, int32 inStartIndex, uint32 inNumElements);

private:

	DataType*				Data = nullptr;
	int32					NumElements = 0;
	uint32					Capacity = 0;
};

#include "Core/Containers/Array.inl"