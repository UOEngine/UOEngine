#pragma once

struct Vector2D
{
			Vector2D()
			{
				X = 0.0f;
				Y = 0.0f;
			}

			Vector2D(float InX, float InY)
			{
				X = InX;
				Y = InY;
			}

			Vector2D(float XY)
			{
				X = XY;
				Y = XY;
			}

			bool operator==(const Vector2D& Other) const
			{
				return X == Other.X && Y == Other.Y;
			}

	float	X;
	float	Y;
};
