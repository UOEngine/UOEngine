#pragma once

#include "Threading/Thread.h"

#include <windows.h>

class WindowsThread: public Thread
{
public:

					WindowsThread();

protected:

	virtual bool	CreateInternal(const Thread::CreationParameters& Parameters) final;

private:

	uint32			Run();

	void*			Params;

	HANDLE			Handle;

	DWORD			Id;

	ThreadFunction	FunctionToRun;

	friend class Thread;

	static DWORD ThreadProc(LPVOID This);

};
