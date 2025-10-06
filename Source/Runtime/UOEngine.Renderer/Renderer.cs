using Microsoft.Xna.Framework.Graphics;

namespace UOEngine.Runtime.Renderer;

public class Renderer
{
    public event Action<RenderContext>? OnFrameBegin;
    public event Action<RenderContext>? OnFrameEnd;

    private readonly GraphicsDevice _graphicsDevice;
    private readonly RenderContext _renderContext;

    public Renderer(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _renderContext = new RenderContext(_graphicsDevice);
    }

    public void RaiseFrameBegin()
    {
        _renderContext.Clear();

        OnFrameBegin?.Invoke(_renderContext);
    }

    public void RaiseFrameEnd()
    {
        OnFrameEnd?.Invoke(_renderContext);
    }
}
