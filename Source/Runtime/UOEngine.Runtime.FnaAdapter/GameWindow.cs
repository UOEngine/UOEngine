namespace Microsoft.Xna.Framework;

public class GameWindow
{
    public IntPtr Handle;
    public Rectangle ClientBounds;

    public event EventHandler<EventArgs> ClientSizeChanged;
}
