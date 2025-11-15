using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.Platform;

namespace Microsoft.Xna.Framework;

public class GameWindow
{
    public IntPtr Handle => _window.Handle;
    public Rectangle ClientBounds;

    public event EventHandler<EventArgs> ClientSizeChanged;

    private readonly IWindow _window;

    public GameWindow()
    {
        _window = FnaAdapterPlugin.Instance.Window;
    }
}
