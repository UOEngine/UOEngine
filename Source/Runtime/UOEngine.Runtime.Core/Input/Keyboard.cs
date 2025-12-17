using System.Collections;

namespace UOEngine.Runtime.Core;

public enum KeyboardKeys
{
    None = 0,
    A,
    B,
    C,
    D, 
    E, 
    F, 
    G,
    H,
    I,
    J,
    K,
    L,
    M,
    N,
    O,
    P,
    Q,
    R,
    S,
    T,
    W,
    X,
    Y,
    Z,

    Count

}
public struct KeyboardState
{
    public BitArray KeyState = new((int)KeyboardKeys.Count);

    public KeyboardState()
    {

    }

    public bool IsKeyPressed(KeyboardKeys key)
    {
        return KeyState[(int)key];
    }

    public bool IsKeyReleased(KeyboardKeys key)
    {
        return KeyState[(int)key] == false;
    }
}
