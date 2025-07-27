#pragma once

#include "Core/Containers/Array.h"

class String
{
public:

					String();

					String(char* Str);

					String(const char* Str);

					String(const String& Other);

					~String();

	String&			operator=(const String& inOther);
	String&			operator=(const char* Str);

	bool			operator==(const char* Str);

	void			Copy(const char* Str, uint32 Length);

	const char*		ToCString() const	{return mData;}

	uint32			Length() const		{return mNumCharacters;}

private:

	struct BufferHeader
	{
		uint32	ReferenceCount;
	};

	struct Buffer
	{
		BufferHeader	Header;
		char			Data[];
	};

	Buffer*			GetBuffer()			{ return (Buffer*)((uint8*)mData - sizeof(BufferHeader)); }
	Buffer*			GetBuffer() const	{ return (Buffer*)((uint8*)mData - sizeof(BufferHeader)); }

	char*			mData;

	uint32			mNumCharacters;

};
