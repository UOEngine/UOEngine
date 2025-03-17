 #pragma once

 #include "Core/Types.h"

class IPlatformWindow
{
public:

	struct CreateParameters
	{
		int32 Event = -1;
	};

	static IPlatformWindow*	Create(const CreateParameters& Parameters);

	virtual void			PollEvents() = 0;
};