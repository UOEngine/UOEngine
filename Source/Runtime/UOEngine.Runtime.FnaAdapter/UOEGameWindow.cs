using Microsoft.Xna.Framework;
using UOEngine.Runtime.Platform;

namespace UOEngine.Runtime.FnaAdapter;

public class UOEGameWindow : GameWindow
{
    private readonly IWindow _window;
    public UOEGameWindow(IWindow window)
    {
        _window = window;
    }

    public override bool AllowUserResizing { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override Rectangle ClientBounds => throw new NotImplementedException();

    public override DisplayOrientation CurrentOrientation { get => throw new NotImplementedException(); internal set => throw new NotImplementedException(); }

    public override nint Handle => _window.Handle;

    public override string ScreenDeviceName => throw new NotImplementedException();

    public override void BeginScreenDeviceChange(bool willBeFullScreen)
    {
        throw new NotImplementedException();
    }

    public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
    {
        throw new NotImplementedException();
    }

    protected override void SetTitle(string title)
    {
        throw new NotImplementedException();
    }

    protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
    {
        throw new NotImplementedException();
    }
}
