namespace UOEngine.Runtime.Core.Input;

public struct MouseState
{
    public bool LeftButtonPressed;
    public bool RightButtonPressed;
}

public class Mouse
{
    public MouseState State => _state;

    private MouseState _state;

    public Mouse(PlatformEventLoop platformEventLoop, ApplicationLoop applicationLoop)
    {
        applicationLoop.OnFrameBegin += OnFrameBegin;
        platformEventLoop.OnMouseButtonDown += OnMouseButtonDown;
    }

    private void OnFrameBegin()
    {
        _state.LeftButtonPressed = false;
        _state.RightButtonPressed = false;
    }

    private void OnMouseButtonDown()
    {

    }
}
