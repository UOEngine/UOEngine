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

	void				Run();

	int					GetReturnCode() {return 0;}

private:

	IPlatformWindow*	mGameWindow;

	uint64				mTicksPassed;

};

extern Engine GEngine;