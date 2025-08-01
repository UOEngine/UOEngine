#include "Core/Containers/String.h"

#include <stdio.h>
#include <cstdarg>
#include <string.h>

#include "Memory/Memory.h"
#include "Memory/MemoryAllocator.h"

int strcmp(const char*, const char*);
int snprintf(char* const _Buffer, size_t      const _BufferCount, char const* const _Format, ...);

String::String()
{
	mData = nullptr;
	mNumCharacters = 0;
}

String::String(char* Str)
{
	mData = nullptr;

	uint32 Length = strlen(Str);

	Copy(Str, Length);
	
}

String::String(const char* Str)
{
	mData = nullptr;

	uint32 Length = strlen(Str);

	Copy(Str, Length);
}

String::String(const String& Other)
{
	mData = Other.mData;
	mNumCharacters = Other.mNumCharacters;

	Buffer* OtherBuffer = Other.GetBuffer();

	GAssert(OtherBuffer->Header.ReferenceCount > 0);

	OtherBuffer->Header.ReferenceCount++;
}

String::~String()
{
	if (mData == nullptr)
	{
		return;
	}

	GetBuffer()->Header.ReferenceCount--;

	if (GetBuffer()->Header.ReferenceCount == 0)
	{
		MemoryAllocator::Get().Free(GetBuffer());

		mData = nullptr;
		mNumCharacters = 0;
	}
}

String& String::operator=(const String& inOther)
{
	if (mData != inOther.mData)
	{
		if (mData != nullptr)
		{
			GNotImplemented;
		}

		inOther.GetBuffer()->Header.ReferenceCount++;

		mData = inOther.mData;
		mNumCharacters = inOther.mNumCharacters;
	}

	return *this;
}

String& String::operator=(const char* Str)
{
	if (Str[0] == 0)
	{
		GNotImplemented;
	}

	uint32 Length = strlen(Str);

	Copy(Str, Length);

	return *this;
}

bool String::operator==(const char* Str)
{
	return (strcmp(mData, Str) == 0);
}

String String::sFormat(const char* inFormat, ...)
{
	const uint32 max_size = 128;
	char output_buffer[max_size];
	va_list args;

	va_start(args, inFormat);
	int32 result = vsnprintf(output_buffer, max_size, inFormat, args);
	va_end(args);

	if (result < 0)
	{
		GAssert(false);
	}

	return String(output_buffer);
}

void String::Copy(const char* Str, uint32 Length)
{
	if (mData != nullptr)
	{
		GNotImplemented;
	}
	else
	{
		mNumCharacters = Length;

		// +1 for terminating zero.
		uint32 StringSizeBytes = (Length + 1) * sizeof(char);

		uint32 SizeInBytes = sizeof(BufferHeader) + StringSizeBytes;

		Buffer* NewBuffer = (Buffer*)MemoryAllocator::Get().Allocate(SizeInBytes);

		NewBuffer->Header.ReferenceCount = 1;

		Memory::MemCopy(NewBuffer->Data, StringSizeBytes, (void*)Str, StringSizeBytes);

		mData = NewBuffer->Data;
	}
}