using UoeKeyboard = UOEngine.Runtime.Core.Input.Keyboard;

namespace Microsoft.Xna.Framework.Input;

public class Keyboard
{
    private static UoeKeyboard _keyboard;

    public Keyboard(UoeKeyboard keyboard)
    {
        _keyboard = keyboard;
    }

    public static KeyboardState GetState()
    {
        return new KeyboardState();
    }
}
