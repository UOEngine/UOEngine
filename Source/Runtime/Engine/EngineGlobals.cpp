#include "Engine/EngineGlobals.h"

namespace
{
	bool GExit = false;
}

void EngineGlobals::RequestExit()
{
	GExit = true;
}

bool EngineGlobals::IsRequestingExit()
{
	return GExit;
}
