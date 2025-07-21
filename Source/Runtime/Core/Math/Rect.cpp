#include "Core/Math/Rect.h"

Rect::Rect()
{
	mLeft = 0;
	mTop = 0;
	mRight = 0;
	mBottom = 0;
}

Rect::Rect(uint32 inLeft, uint32 inTop, uint32 inRight, uint32 inBottom)
{
	mLeft = inLeft;
	mTop = inTop;
	mRight = inRight;
	mBottom = inBottom;
}