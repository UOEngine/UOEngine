#pragma once

typename <DataType>
class TArray<DataType>
{
				TArray();

	DataType	operator[](int32 Index);

private:

	DataType*	Memory;
	int32		NumElements;
};

TArray<DataType>::TArray()
{
	Memory = nullptr;
	NumElements = 0;
}

DataType TArray<DataType>::operator[](int32 Index)
{
	GAssert(Index >= 0);
	GAssert(Index < NumElements);

	return Memory[Index];
}
