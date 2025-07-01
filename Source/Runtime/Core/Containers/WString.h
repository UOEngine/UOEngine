#include "Core/Containers/WString.h"


WString::WString(const String& Str)
{
	mData = nullptr;
	mNumCharacters = 0;

	Copy(Str.ToCString(), Str.Length());
}


void WString::Copy(const char* Str, uint32 Length)
{

}
