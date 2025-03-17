#pragma once

#include "Core/Debug.h"

#define GAssert(Condition,...) do {if(Condition == false) {GBreakpoint;}} while(0)