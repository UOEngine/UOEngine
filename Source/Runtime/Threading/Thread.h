#pragma once

#include "Core/Types.h"

class Thread
{
public:

	using ThreadFunction = void(*)(void* Params);

	struct CreationParameters
	{
		ThreadFunction	Function = nullptr;
		void*			Context = nullptr;
		int32			StackSize = 1024;
	};

	static Thread*	Create(const CreationParameters& CreationParameters);

protected:

	virtual bool	CreateInternal(const CreationParameters& CreationParameters) = 0;

};