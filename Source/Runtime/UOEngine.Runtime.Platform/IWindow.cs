namespace UOEngine.Runtime.Platform;

public interface IWindow
{
    public IntPtr Handle { get; }
    public uint Width { get; }
    public uint Height { get; }

    public uint RenderTargetWidth { get; }
    public uint RenderTargetHeight { get;}

}
