#pragma once

#include "Core/Types.h"

class Rect
{
public:

					Rect();

					Rect(uint32 inLeft, uint32 inTop, uint32 inRight, uint32 inBottom);

	uint32			Left() const			{ return mLeft;}
	uint32			Right() const			{ return mRight;}
	uint32			Bottom() const			{ return mBottom;}
	uint32			Top() const				{ return mTop;}

	uint32			Width() const			{ return mRight - mLeft; }
	uint32			Height() const			{ return mBottom - mTop; }

private:

	uint32			mLeft;																				
	uint32			mTop;																				
	uint32			mRight;																				
	uint32			mBottom;
};