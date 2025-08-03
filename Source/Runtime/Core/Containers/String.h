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

	//template<typename... Args>
	static String	sFormat(const char* inFormat, va_list inArgs);
	static String	sFormat(const char* inFormat, ...);

	void			Copy(const char* Str, uint32 Length);

	const char*		ToCString() const	{return mData;}

	uint32			Length() const		{return mNumCharacters;}

private:

	//static String FormatInplace(const char* inFormat, ...);

	struct BufferHeader
	{
		uint32	ReferenceCount;
	};

	struct Buffer
	{
		BufferHeader	Header;
		char			Data[];

		char*			AddReference()
						{
							Header.ReferenceCount++;

							return Data;
						}

		void			Release() 
						{
							Header.ReferenceCount--;

							if (Header.ReferenceCount == 0)
							{
								MemoryAllocator::Get().Free(this);
							}
						}
	};

	Buffer*			GetBuffer()			{ return (Buffer*)((uint8*)mData - sizeof(Buffer)); }
	Buffer*			GetBuffer() const	{ return (Buffer*)((uint8*)mData - sizeof(Buffer)); }

	char*			mData;

	uint32			mNumCharacters;

};
