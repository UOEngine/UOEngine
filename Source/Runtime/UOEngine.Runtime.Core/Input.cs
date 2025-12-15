using System.Diagnostics;

namespace UOEngine.Runtime.Core;

public class InputManager
{
    public MouseState Mouse => _mouseState[_currentIndex];
    public KeyboardState Keyboard => _keyboardState[_currentIndex];

    private MouseState[] _mouseState = [new(), new()];
    private KeyboardState[] _keyboardState = [new(), new()];

    private int _currentIndex = 0;

    public InputManager(PlatformEventLoop platformEventLoop, ApplicationLoop applicationLoop)
    {
        applicationLoop.OnFrameBegin += OnFrameBegin;

        platformEventLoop.OnMouseButtonDown += OnMouseButtonDown;
        platformEventLoop.OnMouseButtonUp += OnMouseButtonUp;
        platformEventLoop.OnMouseMove += OnMouseMove;
        platformEventLoop.OnMouseWheel += OnMouseWheel;
    }

    public bool IsInputKeyDown(Keys key)
    {
        return false;
    }

    private void OnFrameBegin()
    {
        int previousIndex = _currentIndex;

        _currentIndex ^= _currentIndex;

        _mouseState[_currentIndex] = _mouseState[previousIndex];
        _keyboardState[_currentIndex] = _keyboardState[previousIndex];

    }

    private void OnMouseMove(int x, int y)
    {
        _mouseState[_currentIndex].X = x;
        _mouseState[_currentIndex].Y = y;

        Debug.WriteLine($"{x} {y}");
    }

    private void OnMouseWheel(int delta) => _mouseState[_currentIndex].ScrollWheelDelta += delta;

    private void OnMouseButtonDown(MouseButton button)
    {
        ref var mouseState = ref _mouseState[_currentIndex];

        switch (button)
        {
            case MouseButton.Left: mouseState.LeftButton = ButtonState.Pressed; break;
            case MouseButton.Middle: mouseState.MiddleButton = ButtonState.Pressed; break;
            case MouseButton.Right: mouseState.RightButton = ButtonState.Pressed; break;
        }
    }

    private void OnMouseButtonUp(MouseButton button)
    {
        ref var mouseState = ref _mouseState[_currentIndex];

        switch (button)
        {
            case MouseButton.Left: mouseState.LeftButton = ButtonState.Released; break;
            case MouseButton.Middle: mouseState.MiddleButton = ButtonState.Released; break;
            case MouseButton.Right: mouseState.RightButton = ButtonState.Released; break;
        }
    }
}
