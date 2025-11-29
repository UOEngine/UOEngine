namespace Microsoft.Xna.Framework.Input;

public struct KeyboardState
{
    public bool IsKeyDown(Keys key)
    {
        return false;
    }

    public bool IsKeyUp(Keys key)
    {
        return false;
    }

    public Keys[] GetPressedKeys()
    {
        return [];
    }
}
