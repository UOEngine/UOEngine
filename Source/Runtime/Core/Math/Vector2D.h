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

using IntVector2D = Vector2D<int32>;
