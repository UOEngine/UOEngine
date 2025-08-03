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

	const TArray<DataType>&	operator=(const TArray<DataType>& inRHS);

	DataType*				begin()								{ return mData; }
	DataType*				end()								{ return mData + sizeof(DataType); }

	int32					Add(const DataType& NewElement);

	uint32					GetCapacity() const					{ return mCapacity;}

	bool					Reserve(int32 MinNumElements);

	void					SetNum(uint32 NewSize);

	void					Clear();

	uint32					Num() const;

	DataType*				GetData()							{return mData;}
	const DataType*			GetData() const						{return mData;}

	DataType&				Last()								{return mData[mNumElements - 1]; }

	void					Copy(const DataType* OtherData, uint32 OtherNumElements);
	
	template<class T>
	TSpan<T>				AsSpan()
							{
								uint64 size_of_new_type = sizeof(T);
								uint64 size_of_old_type = sizeof(DataType);
								uint64 conversion = size_of_old_type / size_of_new_type;

								GAssert(conversion > 0); // Fix later when new type > old type.

								uint64 new_size = conversion * mNumElements;

								return TSpan<T>((T*)mData, new_size);
							}

	void					RemoveAt(int32 Index, bool inbShrink);

	void					Move(int32 inIndexFrom, int32 inStartIndex, uint32 inNumElements);

	void					PopBack() {GAssert(mNumElements > 0); mNumElements--;}

private:

	DataType*				mData = nullptr;
	int32					mNumElements = 0;
	uint32					mCapacity = 0;
};

#include "Core/Containers/Array.inl"