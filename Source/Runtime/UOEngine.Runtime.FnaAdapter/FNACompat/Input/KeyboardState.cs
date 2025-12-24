using UOEngineKeyboardState = UOEngine.Runtime.Core.KeyboardState;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.Core;

namespace Microsoft.Xna.Framework.Input;

public struct KeyboardState
{
    private UOEngineKeyboardState _state = FnaAdapterPlugin.Instance.InputManager.Keyboard;
    public KeyboardState()
    {
    }

    public bool IsKeyDown(Keys key)
    {
        return _state.IsKeyPressed(MapKey(key));
    }

    public bool IsKeyUp(Keys key)
    {
        return _state.IsKeyReleased(MapKey(key));
    }

    public Keys[] GetPressedKeys()
    {
        return [];
    }

    private KeyboardKeys MapKey(Keys key)
    {
        if (_fnakeysToKeyboardKeys.TryGetValue(key, out var result))
        {
            return result;
        }

        return KeyboardKeys.None;
    }

    //private static readonly Dictionary<KeyboardKeys, Keys> _keyMap = new()
    //{
    //    [KeyboardKeys.None] = Keys.None,
    //    [KeyboardKeys.A] = Keys.A,
    //    [KeyboardKeys.D] = Keys.D,
    //    [KeyboardKeys.S] = Keys.S,
    //    [KeyboardKeys.W] = Keys.W,
    //};

    private static readonly Dictionary<Keys, KeyboardKeys> _fnakeysToKeyboardKeys = new()
    {
        [Keys.None] = KeyboardKeys.None,
        [Keys.A] = KeyboardKeys.A,
        [Keys.D] = KeyboardKeys.D,
        [Keys.S] = KeyboardKeys.S,
        [Keys.W] = KeyboardKeys.W,
    };
}
