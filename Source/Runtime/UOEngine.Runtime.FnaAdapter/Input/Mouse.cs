using UOEngine.Runtime.Core.Input;
using UOEngine.Runtime.FnaAdapter;

namespace Microsoft.Xna.Framework.Input;

public class Mouse
{
    public static MouseState GetState()
    {
        var state = FnaAdapterPlugin.Instance.InputManager.Mouse.State;

        return new MouseState
        {

        };
    }
}
