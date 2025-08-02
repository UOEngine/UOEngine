 #pragma once

 #include "Core/Math/Vector2D.h"
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
	virtual void*			GetHandle() const = 0;
	virtual void			SetVisible(bool bVisible) = 0;

	virtual IntVector2D		GetExtents() const = 0;

	virtual void			SetTitle(const char* inTitle) = 0;
};