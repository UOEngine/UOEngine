namespace UOEngine.Runtime.Platform;

public interface IWindow
{
    public IntPtr Handle { get; }
    public int Width { get; }
    public int Height { get; }

}
