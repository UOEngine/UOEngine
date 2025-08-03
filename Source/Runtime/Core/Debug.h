#pragma once

#include "Core/Types.h"
//#include "Core/Containers/String.h"

#define GBreakpoint					__debugbreak()
#define GBreakpointIf(condition)	if(condition) {GBreakpoint;}

//void PrintToConsole(const String& inMessage);

void PrintDebugString(const char* inFormat, ...);

//template<typename T>
//constexpr T&& Forward
//
//template<typename T, typename... Args>
//void Test(T value, Args&&... inArgs)
//{
//	bool a = true;
//}
//
//template<typename... Args>
//void PrintDebugString(const char* inFormat, Args&&... inArgs)
//{
//	constexpr uint32 arg_count{ sizeof...(inArgs) };
//	
//	for (int32 i = 0; i < arg_count; i++)
//	{
//		Test(inArgs);
//
//		bool a = true;
//	}
//
//	GBreakpoint;
//	//PrintToConsole(message);
//}
