namespace UOEngine.Runtime.Platform;

public interface IWindow
{
    public event Action<IWindow>? OnResized;

    public IntPtr Handle { get; }
    public uint Width { get; }
    public uint Height { get; }

    public uint RenderTargetWidth { get; }
    public uint RenderTargetHeight { get;}

    public string WindowTitle { get; set; }

}
