using static SDL3.SDL;

using UOEngine.Runtime.Platform;

namespace UOEngine.Runtime.Core;

public class PlatformEventLoop
{
    public event Action<IWindow>? OnWindowResized;
    public event Action? OnMouseButtonDown;

    public bool PollEvents()
    {
        SDL_Event evt;

        while (SDL_PollEvent(out evt))
        {
            switch ((SDL_EventType)evt.type)
            {
                case SDL_EventType.SDL_EVENT_QUIT:
                    {
                        return true;
                    }

                case SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
                    {
                        uint width = (uint)evt.window.data1;
                        uint height = (uint)evt.window.data2;

                        //OnWindowResized?.Invoke(this);

                        break;
                    }

                case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
                    {
                        OnMouseButtonDown?.Invoke();

                        break;
                    }

                default:
                    break;
            }
        }

        return false;
    }
}
