#include "Core/Math/Matrix4x4.h"

Matrix4x4::Matrix4x4()
{
	
}		

Matrix4x4::Matrix4x4(float in00, float in01, float in02, float in03, 
					 float in10, float in11, float in12, float in13, 
					 float in20, float in21, float in22, float in23, 
					 float in30, float in31, float in32, float in33)
{
	mColumns[0] = Vector4(in00, in10, in20, in30);
	mColumns[1] = Vector4(in01, in11, in21, in31);
	mColumns[2] = Vector4(in02, in12, in22, in32);
	mColumns[3] = Vector4(in03, in13, in23, in33);
}

Matrix4x4 Matrix4x4::sCreateOrthographic(float inLeft, float inRight, float inBottom, float inTop, float inNear, float inFar)
{
	float x = 2.0f / (inRight - inLeft);
	float y = 2.0f / (inTop - inBottom);
	float z = -2.0f / (inFar - inNear);
	float tx = -(inRight + inLeft) / (inRight - inLeft);
	float ty = -(inTop + inBottom) / (inTop - inBottom);
	float tz = -(inFar + inNear) / (inFar - inNear);

	return Matrix4x4(   x, 0.0f, 0.0f, tx,
		             0.0f,    y, 0.0f, ty,
		             0.0f, 0.0f,    z, tz,
		             0.0f, 0.0f, 0.0f, 1.0f);
}
