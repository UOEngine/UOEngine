using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UOEngine.Runtime.Renderer;

public class RenderContext
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;

    private UOETexture _texture;

    public RenderContext(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = new SpriteBatch(graphicsDevice);
    }

    public void Clear()
    {
        _graphicsDevice.Clear(Color.CornflowerBlue);
    }

    public void SetTexture(UOETexture texture)
    {
        _texture = texture;
    }

    public void Draw()
    {
        _spriteBatch.Begin();
        _spriteBatch.Draw(_texture.Resource, Vector2.Zero, Color.White);
        _spriteBatch.End();
    }
}
