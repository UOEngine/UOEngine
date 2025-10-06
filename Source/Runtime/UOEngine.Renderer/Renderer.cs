using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UOEngine.Runtime.Renderer;

public class Renderer
{
    public event Action? OnFrameBegin;
    public event Action? OnFrameEnd;

    private readonly GraphicsDevice _graphicsDevice;
    public Renderer(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
    }

    public void Draw(TimeSpan time)
    {
        _graphicsDevice.Clear(Color.Blue);
    }
}
