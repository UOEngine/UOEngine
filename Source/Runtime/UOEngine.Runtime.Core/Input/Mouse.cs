namespace UOEngine.Runtime.Core;

public enum MouseButton
{
    Invalid,
    Left,
    Middle,
    Right,
    Back,
    Forward
}

public enum ButtonState
{
    Released,
    Pressed
}

public struct MouseState
{
    public readonly bool LeftButtonPressed => LeftButton == ButtonState.Pressed;
    public readonly bool MiddleButtonPressed => MiddleButton == ButtonState.Pressed;
    public readonly bool RightButtonPressed => RightButton == ButtonState.Pressed;

    public ButtonState LeftButton;
    public ButtonState MiddleButton;
    public ButtonState RightButton;

    public int X;
    public int Y;

    public int ScrollWheelDelta;
}
