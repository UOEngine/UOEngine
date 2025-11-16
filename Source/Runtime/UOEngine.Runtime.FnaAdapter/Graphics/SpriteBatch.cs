namespace Microsoft.Xna.Framework.Graphics;

public class SpriteBatch
{
    private readonly GraphicsDevice _graphicsDevice;

    public SpriteBatch(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
    }

    public void Begin()
    {
        throw new NotImplementedException();
    }

    public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color)
    {
        throw new NotImplementedException();
    }

    public void Begin(SpriteSortMode sortMode, BlendState blendState)
    {
        throw new NotImplementedException();
    }

    public void Draw(Texture2D texture, Vector2 position, Color color)
    {
        throw new NotImplementedException();
    }

    public void Draw(RenderTarget2D texture, Vector2 position, Color color)
    {
        throw new NotImplementedException();
    }

    public void End()
    {
        throw new NotImplementedException();
    }
}
