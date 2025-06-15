 
 #include "Memory/MemoryAllocator.h"

template <typename DataType>
TArray<DataType>::TArray()
{
	Data = nullptr;
	NumElements = 0;
}

template <typename DataType>
TArray<DataType>::TArray(uint32 InitialCapacity)
{
	uint32 SizeInBytes = InitialCapacity * sizeof(DataType);

	Data = (DataType*)MemoryAllocator::Get().Allocate(SizeInBytes);
	Capacity = InitialCapacity;
	NumElements = 0;
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
		int32 NewSize = 2 * Capacity;

		Reserve(NewSize);
	}

	Data[NumElements] = NewElement;
	NumElements++;

	return NumElements - 1;
}

template <typename DataType>
bool TArray<DataType>::Reserve(int32 MinNumElements)
{
	if (Capacity <= MinNumElements)
	{
		return true;
	}

	const uint32 NewSize = sizeof(DataType) * MinNumElements;

	Data = (DataType*)MemoryAllocator::Get().Reallocate(Data, NewSize);

	Capacity = MinNumElements;
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
