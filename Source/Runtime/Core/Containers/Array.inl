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
	if (Data != nullptr)
	{
		MemoryAllocator::Get().Free(Data);

		Data = nullptr;
		NumElements = 0;
		Capacity = 0;
	}
}

template <typename DataType>
DataType& TArray<DataType>::operator[](int32 Index)
{
	GAssert(Index >= 0);
	GAssert(Index < NumElements);

	return Data[Index];
}

template <typename DataType>
int32 TArray<DataType>::Add(const DataType& NewElement)
{
	if (Capacity == NumElements)
	{
		int32 NewSize = 2 * (Capacity != 0? Capacity: 1);

		Reserve(NewSize);
	}

	Data[NumElements] = NewElement;
	NumElements++;

	return NumElements - 1;
}

template <typename DataType>
bool TArray<DataType>::Reserve(int32 MinNumElements)
{
	if ((Capacity != 0) && (Capacity <= MinNumElements))
	{
		return true;
	}

	const uint32 NewSizeBytes = sizeof(DataType) * MinNumElements;

	if (Data != nullptr)
	{
		Data = (DataType*)MemoryAllocator::Get().Reallocate(Data, NewSizeBytes);
	}
	else
	{
		Data = (DataType*)MemoryAllocator::Get().Allocate(NewSizeBytes);
	}

	uint32 old_size_bytes = NumElements * sizeof(DataType);
	uint32 num_bytes_to_zero = NewSizeBytes - old_size_bytes;

	Memory::MemZero(Data + old_size_bytes, num_bytes_to_zero);

	Capacity = MinNumElements;

	return true;
}

template <typename DataType>
void TArray<DataType>::SetNum(uint32 NewSize)
{
	const uint32 NewSizeBytes = sizeof(DataType) * NewSize;

	if (Data != nullptr)
	{
		Data = (DataType*)MemoryAllocator::Get().Reallocate(Data, NewSizeBytes);
	}
	else
	{
		Data = (DataType*)MemoryAllocator::Get().Allocate(NewSizeBytes);
	}

	NumElements = NewSize;
	Capacity = NumElements;
}

template <typename DataType>
uint32 TArray<DataType>::Num() const
{
	return NumElements;
}

template <typename DataType>
void TArray<DataType>::Copy(const DataType* OtherData, uint32 OtherNumElements)
{
	SetNum(OtherNumElements);

	const uint32 SizeInBytes = OtherNumElements * sizeof(DataType);

	Memory::MemCopy((void*)Data, SizeInBytes, (void*)OtherData, SizeInBytes);

}
