 #pragma once

#include "Engine/EngineGlobals.h"
 #include "Core/Types.h"

 class IPlatformWindow;
 class Thread;

class Engine
{
public:

						Engine();

	bool				Init();
	void				Shutdown();

	void				PreUpdate();
	void				PostUpdate();

	int					GetReturnCode() {return 0;}

private:

	IPlatformWindow*	mGameWindow;
};

extern Engine GEngine;