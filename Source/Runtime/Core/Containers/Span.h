#pragma once

#include "Core/Containers/Array.h"
#include "Core/Types.h"

template <typename DataType>
class TSpan
{
public:

				TSpan(){}

				TSpan(DataType* inData, uint64 inDataLength)
				{
					mData = inData;
					mNumElements = inDataLength;
				}

				//TSpan(const TArray<DataType>& inArray)
				//{
				//	mData = inArray.GetData();
				//	mNumElements = inArray.Num();
				//}

				TSpan(const TSpan<DataType>& inOther)
				{
					mData = inOther.mData;
					mNumElements = inOther.mNumElements;
				}

				~TSpan()
				{
					mData = nullptr;
					mNumElements = 0;
				}

	DataType&	operator[](int32 Index)
				{
					GAssert(Index >= 0);
					GAssert(Index < mNumElements);

					return mData[Index];
				}


	DataType*	GetData() const		{return mData;}

	uint64		Num() const			{return mNumElements;}

private:

	DataType*	mData = nullptr;
	uint64		mNumElements = 0;

};