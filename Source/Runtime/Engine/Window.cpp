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

		int32 ScreenWidth = ::GetSystemMetrics(SM_CXSCREEN);
		int32 ScreenHeight = ::GetSystemMetrics(SM_CYSCREEN);

		RECT WindowRect = { 0, 0, static_cast<LONG>(1920), static_cast<LONG>(1080) };
		
		AdjustWindowRect(&WindowRect, WS_OVERLAPPEDWINDOW, FALSE);

		int32 WindowWidth = WindowRect.right - WindowRect.left;
		int32 WindowHeight = WindowRect.bottom - WindowRect.top;

		int32 WindowX = (ScreenWidth - WindowWidth) / 2;
		int32 WindowY = (ScreenHeight - WindowHeight) / 2;

		// Create the window and store a handle to it.
		Hwnd = CreateWindow(
			windowClass.lpszClassName,
			TEXT("UOEngine"),
			WS_OVERLAPPEDWINDOW,
			WindowX,
			WindowY,
			WindowWidth,
			WindowHeight,
			nullptr,
			nullptr,
			GetModuleHandle(nullptr),
			nullptr);

		return true;
	}

	virtual void SetVisible(bool bVisible)
	{
		::ShowWindow(Hwnd, bVisible);
	}

	virtual void* GetHandle() const {return static_cast<void*>(Hwnd);}

	void PollEvents()
	{
		MSG msg;

		int32 MaxToProcess = 100;

		//while (::GetMessage(&msg, NULL, 0, 0))
		while (MaxToProcess > 0 && PeekMessage(&msg, nullptr, 0, 0, PM_REMOVE))
		{
			::TranslateMessage(&msg);
			::DispatchMessage(&msg);

			MaxToProcess--;
		}
	}

	virtual uint32 GetWidth() const 
	{
		uint32 Width;
		uint32 Height;

		GetWindowRect(Width, Height);

		return Width;
	}

	virtual uint32 GetHeight() const
	{
		uint32 Width;
		uint32 Height;

		GetWindowRect(Width, Height);

		return Height;
	}

private:

	static LRESULT CALLBACK WindowProc(HWND hwnd, UINT Msg, WPARAM wParam, LPARAM lParam)
	{
		switch (Msg)
		{
			case WM_LBUTTONDOWN:
			{
				ReleaseCapture();
				SendMessage(hwnd, WM_NCLBUTTONDOWN, HTCAPTION, 0);
				
				break;
			}

			case WM_SIZE:
			{
				UINT width = LOWORD(lParam);
				UINT height = HIWORD(lParam);

				if (width > 0 && height > 0)
				{
				}

				break;
			}
			break;

			default:
				return DefWindowProc(hwnd, Msg, wParam, lParam);
		}

		return 0;
	}

	void	GetWindowRect(uint32& OutWidth, uint32& OutHeight) const
	{
		RECT Rect;

		::GetClientRect(Hwnd, &Rect);

		OutWidth = Rect.right - Rect.left;
		OutHeight = Rect.bottom - Rect.top;
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