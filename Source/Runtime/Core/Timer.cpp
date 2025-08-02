#include "Core/Timer.h"

#include <windows.h>

Timer sTimer;

uint64 Timer::mTicksPerSecond = 0;
uint64 Timer::mTicksAtProgramStart = 0;

Timer::Timer()
{
	LARGE_INTEGER frequency;

	QueryPerformanceFrequency(&frequency);

	mTicksPerSecond = (uint64)frequency.QuadPart;

	mTicksAtProgramStart = GetTicks();
}

uint64 Timer::GetTicks()
{
	LARGE_INTEGER ticks;

	QueryPerformanceCounter(&ticks);

	return (uint64)ticks.QuadPart;
}

double Timer::sGetTicksInMilliseconds(uint64 inTicks)
{
	double timeMs = 1000.0f * (double)inTicks / (double)mTicksPerSecond;

	return timeMs;
}
