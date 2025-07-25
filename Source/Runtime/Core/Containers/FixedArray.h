#pragma once

template<typename DataType>
class TFixedArray
{
public:

				TFixedArray(DataType* Buffer, int32 Size)
				{
					Memory = Buffer;
					NumElements = Size;
					MaxNumElements = Size;
				}

protected:

	DataType*	Memory;
	int32		NumElements;
	int32		MaxNumElements;
};

