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

	int					GetReturnCode() {return 0;}

private:

	IPlatformWindow*	GameWindow;
};

extern Engine GEngine;