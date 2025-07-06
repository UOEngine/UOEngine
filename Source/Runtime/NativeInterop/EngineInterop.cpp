#include "NativeInterop/EngineInterop.h"

#include "Engine/Engine.h"

int EngineInit()
{
	return GEngine.Init() ? 0 : 1;
}

int EnginePreUpdate()
{
	GEngine.PreUpdate();

	return EngineGlobals::IsRequestingExit() ? 0 : 1;
}

void EnginePostUpdate()
{
	GEngine.PostUpdate();
}

void EngineShutdown()
{
	GEngine.PostUpdate();
}