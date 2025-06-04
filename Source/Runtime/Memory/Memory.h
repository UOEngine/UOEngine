#pragma once

struct Memory
{
	static void MemZero(void* Data, uint32 SizeInBytes)
	{
		memset(Data, 0, SizeInBytes);
	}
};