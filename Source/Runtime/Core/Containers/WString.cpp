#pragma once

#include "Core/Containers/String.h"

class WString
{
public:

					WString(const String& Str);

	void			Copy(const char* Str, uint32 Length);

private:

	wchar_t*		mData;

	uint32			mNumCharacters;
};
