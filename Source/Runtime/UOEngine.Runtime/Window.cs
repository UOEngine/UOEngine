using Microsoft.Xna.Framework;
using SDL3;

using UOEngine.Runtime.Platform;

namespace UOEngine.Runtime;

public class Window: IWindow
{
    public IntPtr Handle { get; }

    public int Width => _gameWindow.ClientBounds.Width;
    public int Height => _gameWindow.ClientBounds.Height;


    private readonly GameWindow _gameWindow;

    public Window(GameWindow gameWindow)
    {
        _gameWindow = gameWindow;

        Handle = SDL.SDL_GetPointerProperty(SDL.SDL_GetWindowProperties(gameWindow.Handle), SDL.SDL_PROP_WINDOW_WIN32_HWND_POINTER, IntPtr.Zero);
    }
}
