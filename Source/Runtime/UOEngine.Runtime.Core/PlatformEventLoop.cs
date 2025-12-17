using static SDL3.SDL;

using UOEngine.Runtime.Platform;

namespace UOEngine.Runtime.Core;

public class PlatformEventLoop
{
    public event Action? OnQuit;
    public event Action<IWindow, uint, uint>? OnWindowResized;
    public event Action<int, int>? OnMouseMove;
    public event Action<MouseButton>? OnMouseButtonDown;
    public event Action<MouseButton>? OnMouseButtonUp;
    public event Action<int>? OnMouseWheel;
    public event Action<KeyboardKeys>? OnKeyDown;
    public event Action<KeyboardKeys>? OnKeyUp;
    public event Action<char>? OnTextInput;
    public event Action? OnFocusLost;

    private IWindow _window;
    public void RegisterWindow(IWindow window)
    {
        _window = window;
    }

    public bool PollEvents()
    {
        SDL_Event evt;

        while (SDL_PollEvent(out evt))
        {
            switch ((SDL_EventType)evt.type)
            {
                case SDL_EventType.SDL_EVENT_QUIT:
                    {
                        OnQuit?.Invoke();

                        return true;
                    }

                case SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
                    {
                        uint width = (uint)evt.window.data1;
                        uint height = (uint)evt.window.data2;

                        OnWindowResized?.Invoke(_window, width, height);

                        break;
                    }

                case SDL_EventType.SDL_EVENT_WINDOW_FOCUS_LOST:
                    {
                        break;
                    }

                case SDL_EventType.SDL_EVENT_WINDOW_FOCUS_GAINED:
                    {
                        break;
                    }

                case SDL_EventType.SDL_EVENT_MOUSE_WHEEL:
                    {
                        int y = (int)evt.wheel.y;

                        OnMouseWheel?.Invoke(y);
                        break;
                    }

                case SDL_EventType.SDL_EVENT_MOUSE_MOTION:
                    {
                        int x = (int)evt.motion.x;
                        int y = (int)evt.motion.y;

                        OnMouseMove?.Invoke(x, y);

                        break;
                    }

                case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
                    {
                        for (int i = 0; i < (int)SDL_MouseButtonFlags.SDL_BUTTON_X2MASK; i++)
                        {
                            int button = (evt.button.button & (1 << i));

                            if (button != 0)
                            {
                                OnMouseButtonDown?.Invoke(MapMouseButton((SDL_MouseButtonFlags)button));
                            }
                        }

                        break;
                    }

                case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP:
                    {
                        for (int i = 0; i < (int)SDL_MouseButtonFlags.SDL_BUTTON_X2MASK; i++)
                        {
                            int button = (evt.button.button & (1 << i));

                            if (button != 0)
                            {
                                OnMouseButtonUp?.Invoke(MapMouseButton((SDL_MouseButtonFlags)button));
                            }
                        }

                        break;
                    }

                case SDL_EventType.SDL_EVENT_KEY_DOWN:
                    {
                        // TODO: key code vs scan code.
                        var key = MapKeyboardKey((SDL_Keycode)evt.key.key);

                        OnKeyDown?.Invoke(key);

                        break;
                    }

                case SDL_EventType.SDL_EVENT_KEY_UP:
                    {
                        var key = MapKeyboardKey((SDL_Keycode)evt.key.key);

                        OnKeyUp?.Invoke(key);

                        break;
                    }

                default:
                    break;
            }
        }

        return false;
    }

    private KeyboardKeys MapKeyboardKey(SDL_Keycode keycode)
    {
        if(_keyMap.TryGetValue(keycode, out var key))
        {
            return key;
        }

        return KeyboardKeys.None;
    }

    private static MouseButton MapMouseButton(SDL_MouseButtonFlags button) => button switch
    {
        SDL_MouseButtonFlags.SDL_BUTTON_LMASK => MouseButton.Left,
        SDL_MouseButtonFlags.SDL_BUTTON_MMASK => MouseButton.Middle,
        SDL_MouseButtonFlags.SDL_BUTTON_RMASK => MouseButton.Right,
        SDL_MouseButtonFlags.SDL_BUTTON_X1MASK => MouseButton.Back,
        SDL_MouseButtonFlags.SDL_BUTTON_X2MASK => MouseButton.Forward,
        _ => throw new NotImplementedException()
    };

    private static readonly Dictionary<SDL_Keycode, KeyboardKeys> _keyMap = new()
    {
        [SDL_Keycode.SDLK_A] = KeyboardKeys.A,
        [SDL_Keycode.SDLK_D] = KeyboardKeys.D,
        [SDL_Keycode.SDLK_S] = KeyboardKeys.S,
        [SDL_Keycode.SDLK_W] = KeyboardKeys.W,

    };

}
