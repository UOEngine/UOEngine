using static SDL3.SDL;

using UOEngine.Runtime.Platform;

namespace UOEngine.Runtime.Core;

public class Window: IWindow, IDisposable
{
    public string WindowTitle
    {
        set => SDL_SetWindowTitle(Handle, value);
        get => SDL_GetWindowTitle(Handle);
    }

    public event Action<IWindow>? OnResized;

    public IntPtr Handle { get; private set; }

    public uint Width { get; private set; }
    public uint Height { get; private set; }

    public uint RenderTargetWidth { get; private set; }
    public uint RenderTargetHeight { get; private set; }

    private bool _disposed;

    public void Startup(PlatformEventLoop eventLoop)
    {
        if (!SDL_Init(SDL_InitFlags.SDL_INIT_EVENTS))
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

        eventLoop.RegisterWindow(this);

        // Win32 handle if needed
        //Handle = SDL.SDL_GetPointerProperty(SDL.SDL_GetWindowProperties(_sdlHandle), SDL.SDL_PROP_WINDOW_WIN32_HWND_POINTER, IntPtr.Zero);

    }

    public void UpdateRenderTargetSize()
    {
        RenderTargetWidth = Width;
        RenderTargetHeight = Height;
    }

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if(_disposed)
        {
            return;
        }

        SDL_DestroyWindow(Handle);
        SDL_QuitSubSystem(SDL_InitFlags.SDL_INIT_VIDEO);

        _disposed = true;
    }

    private void OnWindowResize(IWindow window, uint width, uint height)
    {
        Width = width;
        Height = height;

        UpdateRenderTargetSize();

        OnResized?.Invoke(window);
    }
}
