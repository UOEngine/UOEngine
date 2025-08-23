#pragma once

#include "Core/Types.h"

template<typename T>
struct Vector2D
{
			Vector2D()
			{
				X = 0;
				Y = 0;
			}

			Vector2D(T InX, T InY)
			{
				X = InX;
				Y = InY;
			}

			Vector2D(T XY)
			{
				X = XY;
				Y = XY;
			}

			bool operator==(const Vector2D<T>& Other) const
			{
				return X == Other.X && Y == Other.Y;
			}

	T	X;
	T	Y;
};

struct IntVector2D
{
	IntVector2D()
	{
		X = 0;
		Y = 0;
	}

	IntVector2D(int32 InX, int32 InY)
	{
		X = InX;
		Y = InY;
	}

	IntVector2D(int32 XY)
	{
		X = XY;
		Y = XY;
	}

	bool operator==(const IntVector2D& Other) const
	{
		return X == Other.X && Y == Other.Y;
	}

	int32	X;
	int32	Y;
};

