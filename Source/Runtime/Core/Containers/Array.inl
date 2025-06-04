 
 #include "Memory/Allocator.h"

template <typename DataType, class AllocatorType>
TArray<DataType, AllocatorType>::TArray()
{
	Data = nullptr;
	NumElements = 0;
}

template <typename DataType, class AllocatorType/*=Allocator*/>
TArray<DataType, AllocatorType>::TArray(uint32 InitialCapacity)
{
	uint32 SizeInBytes = InitialCapacity * sizeof(DataType);

	Data = static_cast<DataType*>(AllocatorType::Allocate(SizeInBytes));
	Capacity = InitialCapacity;
	NumElements = 0;
}

template <typename DataType, class AllocatorType>
DataType& TArray<DataType, AllocatorType>::operator[](int32 Index)
{
	GAssert(Index >= 0);
	GAssert(Index < NumElements);

	return Data[Index];
}

template <typename DataType, class AllocatorType>
int32 TArray<DataType, AllocatorType>::Add(const DataType& NewElement)
{
	if (Capacity == NumElements)
	{
		int32 NewSize = 2 * Capacity;

		Resize(NewSize);
	}

	Data[NumElements] = NewElement;
	NumElements++;

	return NumElements - 1;
}

template <typename DataType, class AllocatorType/*=Allocator*/>
void TArray<DataType, AllocatorType>::Resize(uint32 NewSize)
{
	GAssert(false);
}

template <typename DataType, class AllocatorType>
bool TArray<DataType, AllocatorType>::Reserve(int32 MinNumElements)
{
	if (Capacity <= MinNumElements)
	{
		return true;
	}

	void* NewAddress = AllocatorType::Reallocate(Data, MinNumElements);

	Data = NewAddress;

}

template <typename DataType, class AllocatorType>
void TArray<DataType, AllocatorType>::SetNum(uint32 NewSize)
{
	Data = static_cast<DataType*>(AllocatorType::Reallocate(Data, sizeof(DataType) * NewSize));
}

template <typename DataType, class AllocatorType>
uint32 TArray<DataType, AllocatorType>::Num() const
{
	return NumElements;
}
