using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UOEngine.Runtime.Renderer;

public class RenderFactory
{
    private readonly GraphicsDevice _graphicsDevice;

    public RenderFactory(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
    }

    public UOETexture CreateTexture(int width, int height)
    {
        var texture = new UOETexture(_graphicsDevice, width, height);

        return texture;
    }
}
