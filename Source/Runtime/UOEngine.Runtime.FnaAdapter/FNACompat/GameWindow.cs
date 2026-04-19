using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.Platform;

namespace Microsoft.Xna.Framework;

public class GameWindow
{
    public IntPtr Handle => _host.NativeWindowHandle;
    public Rectangle ClientBounds => GetWindowBounds();

    public event EventHandler<EventArgs>? ClientSizeChanged;

    private readonly IHostedGameHost _host;

    public GameWindow(IHostedGameHost host)
    {
        _host = host;

        _host.BoundsChanged += _ => ClientSizeChanged?.Invoke(this, EventArgs.Empty);
    }

    public Rectangle GetWindowBounds()
    {
        return new Rectangle(0, 0, _host.ClientBounds.Width, _host.ClientBounds.Height);
    }
}
