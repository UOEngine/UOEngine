using static SDL3.SDL;

using UOEngine.Runtime.Platform;

namespace UOEngine.Runtime.Core;

public class Window: IWindow
{
    public event Action<IWindow>? OnResized;

    public IntPtr Handle { get; private set; }

    public uint Width { get; private set; }
    public uint Height { get; private set; }

    public uint RenderTargetWidth { get; private set; }
    public uint RenderTargetHeight { get; private set; }

    public void Startup(PlatformEventLoop eventLoop)
    {
        if (!SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO))
        {
            throw new Exception("SDL_Init failed: " + SDL_GetError());
        }

        eventLoop.OnWindowResized += OnWindowResize;

        string title = "UOEngine";

        SDL_WindowFlags initFlags = SDL_WindowFlags.SDL_WINDOW_RESIZABLE;

        Width = 1920;
        Height = 1080;

        Handle = SDL_CreateWindow(title, (int)Width, (int)Height, initFlags);

        if (Handle == IntPtr.Zero)
        {
            throw new Exception("SDL_CreateWindow failed: " + SDL_GetError());
        }

        UpdateRenderTargetSize();

        // Win32 handle if needed
        //Handle = SDL.SDL_GetPointerProperty(SDL.SDL_GetWindowProperties(_sdlHandle), SDL.SDL_PROP_WINDOW_WIN32_HWND_POINTER, IntPtr.Zero);

    }

    public void UpdateRenderTargetSize()
    {
        SDL_GetWindowSizeInPixels(Handle, out var width, out var height);

        RenderTargetWidth = (uint)width;
        RenderTargetHeight = (uint)height;
    }

    public void Dispose()
    {
        SDL_DestroyWindow(Handle);
        SDL_QuitSubSystem(SDL_InitFlags.SDL_INIT_VIDEO);
    }

    private void OnWindowResize(IWindow window)
    {
        SDL_GetWindowSize(Handle, out int width, out int height);

        Width = (uint)width;
        Height = (uint)height;

        UpdateRenderTargetSize();

        OnResized?.Invoke(window);
    }
}
