using UoeKeyboard = UOEngine.Runtime.Core;

namespace Microsoft.Xna.Framework.Input;

public class Keyboard
{

    public Keyboard()
    {
    }

    public static KeyboardState GetState()
    {
        return new KeyboardState();
    }
}
