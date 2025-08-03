#include "Core/Debug.h"

#include <windows.h>
#include <cstdarg>

#include "Core/Containers/String.h"

void PrintToConsole(const String& inMessage)
{
	HANDLE hConsole = GetStdHandle(STD_OUTPUT_HANDLE);

	DWORD written;

	WriteConsoleA(hConsole, inMessage.ToCString(), (DWORD)inMessage.Length(), &written, nullptr);
}

void PrintDebugString(const char* inFormat, ...)
{
	va_list args;

	va_start(args, inFormat);
	String debug_string = String::sFormat(inFormat, args);
	va_end(args);

	debug_string = String::sFormat("%s\n", debug_string.ToCString());

	OutputDebugStringA(debug_string.ToCString());

}
