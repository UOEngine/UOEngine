#include "Core/Containers/String.h"

#include <string.h>

#include "Memory/Memory.h"
#include "Memory/MemoryAllocator.h"

String::String()
{

}

String::String(char* Str)
{
	Data = nullptr;

	uint32 Length = strlen(Str);

	Copy(Str, Length);
	
}

String::String(const char* Str)
{
	Data = nullptr;

	uint32 Length = strlen(Str);

	Copy(Str, Length);
}

String::String(const String& Other)
{
	Data = Other.Data;
	NumCharacters = Other.NumCharacters;

	Buffer* OtherBuffer = Other.GetBuffer();

	GAssert(OtherBuffer->Header.ReferenceCount > 0);

	OtherBuffer->Header.ReferenceCount++;
}

String::~String()
{
	GetBuffer()->Header.ReferenceCount--;

	if (GetBuffer()->Header.ReferenceCount == 0)
	{
		MemoryAllocator::Get().Free(GetBuffer());

		Data = nullptr;
	}
}

void String::Copy(const char* Str, uint32 Length)
{
	if (Data != nullptr)
	{
		GAssert(false);
	}
	else
	{
		NumCharacters = Length;

		// +1 for terminating zero.
		uint32 StringSizeBytes = (Length + 1) * sizeof(char);

		uint32 SizeInBytes = sizeof(BufferHeader) + StringSizeBytes;

		Buffer* NewBuffer = (Buffer*)MemoryAllocator::Get().Allocate(SizeInBytes);

		NewBuffer->Header.ReferenceCount = 1;

		Memory::MemCopy(NewBuffer->Data, StringSizeBytes, (void*)Str, StringSizeBytes);

		Data = NewBuffer->Data;
	}
}