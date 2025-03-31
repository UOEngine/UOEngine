#pragma once

#include "Core/Debug.h"

#define GAssert(Condition,...)	do {if(Condition == false) {GBreakpoint;}} while(0)
#define GCrash(...)				do{*((int*)0) = 0xdeadbeef;} while(0)