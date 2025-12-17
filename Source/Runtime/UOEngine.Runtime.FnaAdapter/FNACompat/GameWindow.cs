using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.Platform;

namespace Microsoft.Xna.Framework;

public class GameWindow
{
    public IntPtr Handle => _window.Handle;
    public Rectangle ClientBounds => GetWindowBounds();

    public event EventHandler<EventArgs> ClientSizeChanged;

    private readonly IWindow _window;

    public GameWindow(IWindow window)
    {
        _window = window;

        _window.OnResized += OnWindowResized;

    }

    private void OnWindowResized(IWindow window)
    {
        ClientSizeChanged?.Invoke(this, EventArgs.Empty);
    }

    public Rectangle GetWindowBounds()
    {
        return new Rectangle(0, 0, (int)_window.Width, (int)_window.Height);
    }
}
