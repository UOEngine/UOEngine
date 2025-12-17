using UOEngine.Runtime.FnaAdapter;

namespace Microsoft.Xna.Framework.Input;

public class Keyboard
{

    public Keyboard()
    {
    }

    public static KeyboardState GetState()
    {
        //var state = FnaAdapterPlugin.Instance.InputManager.Keyboard;

        return new KeyboardState();
    }
}
