#pragma once

#include "Core/Types.h"

struct Vector4
{
			Vector4(float inX, float inY, float inZ, float inW)
			{
				X  = inX;
				Y = inY;
				Z = inZ;
				W = inW;
			}

	float	X;
	float	Y;
	float	Z;
	float	W;
};

class Matrix4x4
{
public:

								Matrix4x4();

								Matrix4x4(float in00, float in01, float in02, float in03,
										  float in10, float in11, float in12, float in13,
										  float in20, float in21, float in22, float in23,
										  float in30, float in31, float in32, float in33);

	static Matrix4x4			sCreateOrthographic(float inLeft, float inRight, float inBottom, float inTop, float inNear, float inFar);

	static constexpr uint32		Rows = 4;
	static constexpr uint32		Columns = 4;
	static constexpr uint32		NumElements = Rows * Columns;

private:

	union
	{
		struct
		{
			Vector4 mColumns[Columns];
		};

		float		mElements[NumElements];
	};
};