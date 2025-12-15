using UOEngine.Runtime.FnaAdapter;

namespace Microsoft.Xna.Framework.Input;

public class Mouse
{
    public static MouseState GetState()
    {
        var inputManager = FnaAdapterPlugin.Instance.InputManager;

        var state = FnaAdapterPlugin.Instance.InputManager.Mouse;

        return new MouseState
        {
            LeftButton = state.LeftButtonPressed ? ButtonState.Pressed : ButtonState.Released,
            MiddleButton = state.MiddleButtonPressed? ButtonState.Pressed: ButtonState.Released,
            RightButton = state.RightButtonPressed ? ButtonState.Pressed : ButtonState.Released,
            X = state.X,
            Y = state.Y,
            ScrollWheelValue = state.ScrollWheelDelta
        };
    }
}
