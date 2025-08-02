#pragma once

#include "Core/Types.h"

class Timer
{
public:

	Timer();

	static uint64 GetTicks();

	static double sGetTicksInMilliseconds(uint64 inTicks);

private:

	static uint64 mTicksPerSecond;
	static uint64 mTicksAtProgramStart;

};