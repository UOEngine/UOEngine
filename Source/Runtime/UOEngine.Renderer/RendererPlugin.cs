using UOEngine.Plugin;

namespace UOEngine.Renderer;

public class RendererPlugin : IPlugin
{
    public event EventHandler? OnFrameBegin;
    public event EventHandler? OnFrameEnd;

    public void Startup()
    {
    }
}
