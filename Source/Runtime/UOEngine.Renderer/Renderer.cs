using Microsoft.Xna.Framework.Graphics;

namespace UOEngine.Runtime.Renderer;

public class Renderer
{
    public event Action<RenderContext>? OnFrameBegin;
    public event Action<RenderContext>? OnFrameEnd;

    private readonly GraphicsDevice _graphicsDevice;
    public readonly RenderContext RenderContext;

    public Renderer()
    {
        //_graphicsDevice = graphicsDevice;
        //RenderContext = new RenderContext(_graphicsDevice);
    }

    public void RaiseFrameBegin()
    {
        RenderContext.Clear();

        OnFrameBegin?.Invoke(RenderContext);
    }

    public void RaiseFrameEnd()
    {
        OnFrameEnd?.Invoke(RenderContext);
    }
}
