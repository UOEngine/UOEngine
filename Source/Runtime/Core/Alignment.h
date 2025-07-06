#pragma once

namespace Alignment
{
	template<typename T>
	T constexpr Align(T Size, uint64 Alignment)
	{
		return ((uint64)Size + (uintptr_t(Alignment) - 1)) & (~(uintptr_t(Alignment) - 1));
	}
}