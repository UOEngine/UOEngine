 #pragma once

 #include "Core/Types.h"

 class IPlatformWindow;
 class Thread;

class Engine
{
public:

						Engine();

	bool				Init();

	void				Run();

	void				RequestExit()	{bExitRequested = true;}

	int					GetReturnCode() {return 0;}

private:

	IPlatformWindow*	GameWindow;

	bool				bExitRequested;
};

extern Engine GEngine;