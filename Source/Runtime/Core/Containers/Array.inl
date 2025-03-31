 
template <typename DataType, class AllocatorType>
DataType TArray<DataType, AllocatorType>::operator[](int32 Index)
{
	GAssert(Index >= 0);
	GAssert(Index < NumElements);

	return Memory[Index];
}

template <typename DataType, class AllocatorType>
int32 TArray<DataType, AllocatorType>::Add(int32 Index)
{
	if (Capacity() == NumElements)
	{
		int32 NewSize = 2 * Capacity();

		Resize(NewSize);
	}
}

template <typename DataType, class AllocatorType>
int32 TArray<DataType, AllocatorType>::Capacity() const
{

}

template <typename DataType, class AllocatorType/*=Allocator*/>
bool TArray<DataType, AllocatorType>::Reserve(int32 MinNumElements)
{
	if (Capacity() <= MinNumElements)
	{
		return true;
	}

	void* NewAddress = AllocatorType::Reallocate(MinNumElements);

	Data = TFixedArray<DataType>(NewAddress, MinNumElements);

}


template <typename DataType, class AllocatorType/*=Allocator*/>
void TArray<DataType, AllocatorType>::Resize(int32 NewSizeBytes)
{

}
