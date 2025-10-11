using Microsoft.Xna.Framework;

namespace UOEngine.Runtime;

public class Window
{
    public IntPtr Handle { get; private set; }

    public Window(GameWindow gameWindow)
    {
        Handle = gameWindow.Handle;
    }
}
