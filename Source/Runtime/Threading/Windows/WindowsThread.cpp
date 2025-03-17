#include "Threading/Windows/WindowsThread.h" 

Thread* Thread::Create(const Thread::CreationParameters& CreationParameters)
{
	WindowsThread* WinThread = new WindowsThread();

	if (WinThread->CreateInternal(CreationParameters))
	{
		return WinThread;
	}

	return nullptr;
}

WindowsThread::WindowsThread()
{
	Params = nullptr;
	Handle = INVALID_HANDLE_VALUE;
	Id = -1;
	FunctionToRun = nullptr;
}

bool WindowsThread::CreateInternal(const Thread::CreationParameters& CreationParameters)
{
	Params = CreationParameters.Context;
	FunctionToRun = CreationParameters.Function;

	Handle = ::CreateThread(nullptr, CreationParameters.StackSize, &WindowsThread::ThreadProc, this, CREATE_SUSPENDED, &Id);

	if (Handle == INVALID_HANDLE_VALUE)
	{
		return false;
	}

	return true;
}

uint32 WindowsThread::Run()
{
	FunctionToRun(Params);

	return 0;
}

DWORD WindowsThread::ThreadProc(LPVOID This)
{
	WindowsThread* ThisThread = (WindowsThread*)This;

	return ThisThread->Run();
}
