 #include "Memory/Memory.h"
 #include "Memory/MemoryAllocator.h"

template <typename DataType>
TArray<DataType>::TArray()
{
	Reserve(4);
}

template <typename DataType>
TArray<DataType>::TArray(std::initializer_list<DataType> InitList)
{
	Copy(InitList.begin(), InitList.size());
}

template <typename DataType>
TArray<DataType>::TArray(const DataType* Ptr, uint32 NumToCopy)
{
	Copy(Ptr, NumToCopy);
}

template <typename DataType>
TArray<DataType>::~TArray()
{
	Clear();
}

template <typename DataType>
DataType& TArray<DataType>::operator[](int32 Index)
{
	GAssert(Index >= 0);
	GAssert(Index < mNumElements);

	return mData[Index];
}

template <typename DataType>
const DataType& TArray<DataType>::operator[](int32 Index) const
{
	GAssert(Index >= 0);
	GAssert(Index < mNumElements);

	return mData[Index];
}

template <typename DataType>
const TArray<DataType>& TArray<DataType>::operator=(const TArray<DataType>& inRHS)
{
	Clear();

	Copy(inRHS.GetData(), inRHS.Num());

	return *this;
}

template <typename DataType>
int32 TArray<DataType>::Add(const DataType& NewElement)
{
	if (mCapacity == mNumElements)
	{
		int32 NewSize = 2 * (mCapacity != 0? mCapacity: 1);

		Reserve(NewSize);
	}

	mData[mNumElements] = NewElement;
	mNumElements++;

	return mNumElements - 1;
}

template <typename DataType>
bool TArray<DataType>::Reserve(int32 MinNumElements)
{
	if ((mCapacity != 0) && (mCapacity <= MinNumElements))
	{
		return true;
	}

	const uint32 NewSizeBytes = sizeof(DataType) * MinNumElements;

	if (mData != nullptr)
	{
		mData = (DataType*)MemoryAllocator::Get().Reallocate(mData, NewSizeBytes);
	}
	else
	{
		mData = (DataType*)MemoryAllocator::Get().Allocate(NewSizeBytes);
	}

	uint64 old_size_bytes = mNumElements * sizeof(DataType);
	uint64 num_bytes_to_zero = NewSizeBytes - old_size_bytes;

	Memory::MemZero(mData + old_size_bytes, num_bytes_to_zero);

	mCapacity = MinNumElements;

	return true;
}

template <typename DataType>
void TArray<DataType>::SetNum(uint32 NewSize)
{
	const uint32 NewSizeBytes = sizeof(DataType) * NewSize;

	if (mData != nullptr)
	{
		mData = (DataType*)MemoryAllocator::Get().Reallocate(mData, NewSizeBytes);
	}
	else
	{
		mData = (DataType*)MemoryAllocator::Get().Allocate(NewSizeBytes);
	}

	mNumElements = NewSize;
	mCapacity = mNumElements;
}

template <typename DataType>
void TArray<DataType>::Clear()
{
	if (mData != nullptr)
	{
		MemoryAllocator::Get().Free(mData);

		mData = nullptr;
		mNumElements = 0;
		mCapacity = 0;
	}
}

template <typename DataType>
uint32 TArray<DataType>::Num() const
{
	return mNumElements;
}

template <typename DataType>
void TArray<DataType>::Copy(const DataType* OtherData, uint32 OtherNumElements)
{
	SetNum(OtherNumElements);

	const uint32 SizeInBytes = OtherNumElements * sizeof(DataType);

	Memory::MemCopy((void*)mData, SizeInBytes, (void*)OtherData, SizeInBytes);

}

template <typename DataType>
void TArray<DataType>::RemoveAt(int32 inIndex, bool inbShrink)
{
	uint32 num_to_move = 1;

	if (Num() > 1)
	{
		Move(inIndex + 1, inIndex, num_to_move);
	}

	mNumElements--;
}

template <typename DataType>
void TArray<DataType>::Move(int32 inIndexFrom, int32 inStartIndex, uint32 inNumElements)
{
	for (uint32 i = 0; i < mNumElements; i++)
	{
		mData[inStartIndex + i] = mData[inIndexFrom + i];
	}
}
