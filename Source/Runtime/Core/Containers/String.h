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

	void			Copy(const char* Str, uint32 Length);

	const char*		ToCString() const {return Data;}

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

	Buffer* GetBuffer()			{ return (Buffer*)((uint8*)Data - sizeof(BufferHeader)); }
	Buffer* GetBuffer() const	{ return (Buffer*)((uint8*)Data - sizeof(BufferHeader)); }

	char*	Data;

	uint32 NumCharacters;

};

