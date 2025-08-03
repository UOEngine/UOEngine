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

	OtherBuffer->AddReference();
}

String::~String()
{
	if (mData == nullptr)
	{
		return;
	}

	GetBuffer()->Release();

	if (GetBuffer()->Data == nullptr)
	{
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
			GetBuffer()->Release();
		}

		inOther.GetBuffer()->AddReference();

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

String String::sFormat(const char* inFormat, va_list inArgs)
{
	const uint32 max_size = 128;
	char output_buffer[max_size] = {0};
	//va_list args;

	//va_start(args, inFormat);
	int32 result = vsnprintf(output_buffer, max_size, inFormat, inArgs);
	//va_end(args);

	if (result < 0)
	{
		GAssert(false);
	}

	return String(output_buffer);
}

String String::sFormat(const char* inFormat, ...)
{
	va_list args;

	va_start(args, inFormat);
	String str = String::sFormat(inFormat, args);
	va_end(args);

	return str;
}

void String::Copy(const char* inStr, uint32 inLength)
{
	if (mData != nullptr)
	{
		GNotImplemented;
	}
	else
	{
		mNumCharacters = inLength;

		// +1 for terminating zero.
		uint32 string_size_bytes = (inLength + 1) * sizeof(char);

		uint32 size_in_bytes = sizeof(Buffer) + string_size_bytes;

		Buffer* new_buffer = (Buffer*)MemoryAllocator::Get().Allocate(size_in_bytes);

		new_buffer->Header.ReferenceCount = 1;

		Memory::MemCopy(new_buffer->Data, string_size_bytes, (void*)inStr, string_size_bytes);

		mData = new_buffer->Data;
	}
}