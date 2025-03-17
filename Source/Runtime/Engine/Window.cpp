#include "Engine/Window.h"

#include <windows.h>

class Win32Window: public IPlatformWindow
{
public:

	Win32Window()
	{
		Hwnd = nullptr;
	}

	bool Create(const CreateParameters& Parameters)
	{
		// Initialize the window class.
		WNDCLASSEX windowClass = { 0 };

		windowClass.cbSize = sizeof(WNDCLASSEX);
		windowClass.style = CS_HREDRAW | CS_VREDRAW;
		windowClass.lpfnWndProc = &Win32Window::WindowProc;
		windowClass.hInstance = GetModuleHandle(nullptr);
		windowClass.hCursor = LoadCursor(NULL, IDC_ARROW);
		windowClass.lpszClassName = L"UOEngine";

		RegisterClassEx(&windowClass);

		RECT windowRect = { 0, 0, static_cast<LONG>(1920), static_cast<LONG>(1080) };
		
		AdjustWindowRect(&windowRect, WS_OVERLAPPEDWINDOW, FALSE);

		// Create the window and store a handle to it.
		Hwnd = CreateWindow(
			windowClass.lpszClassName,
			TEXT("UOEngine"),
			WS_OVERLAPPEDWINDOW,
			CW_USEDEFAULT,
			CW_USEDEFAULT,
			windowRect.right - windowRect.left,
			windowRect.bottom - windowRect.top,
			nullptr,
			nullptr,
			GetModuleHandle(nullptr),
			nullptr);

		ShowWindow(Hwnd, true);

		return true;
	}

	void PollEvents()
	{
		MSG msg;

		int32 MaxToProcess = 100;

		//while (::GetMessage(&msg, NULL, 0, 0))
		while(MaxToProcess > 0 && PeekMessage(&msg, nullptr, 0, 0, PM_NOREMOVE))
		{
			::TranslateMessage(&msg);
			::DispatchMessage(&msg);

			MaxToProcess--;
		}
	}

private:

	static LRESULT CALLBACK WindowProc(HWND hwnd, UINT Msg, WPARAM wParam, LPARAM lParam)
	{
		switch (Msg)
		{
			

		}

		return DefWindowProc(hwnd, Msg, wParam, lParam);
	}

	HWND Hwnd;
};

IPlatformWindow* IPlatformWindow::Create(const CreateParameters& Parameters)
{
	Win32Window* Window = new Win32Window();

	if (Window->Create(Parameters))
	{
		return Window;
	}

	return nullptr;
}