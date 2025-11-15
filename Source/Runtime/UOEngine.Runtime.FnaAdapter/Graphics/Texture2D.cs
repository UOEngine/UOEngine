namespace Microsoft.Xna.Framework.Graphics;

public class Texture2D: Texture
{
    public int Width;
    public int Height;

    public Texture2D(
        GraphicsDevice graphicsDevice,
        int width,
        int height,
        bool mipMap,
        SurfaceFormat format
    )
    {
        throw new NotImplementedException();
    }

    public void SetData<T>(T[] data) where T : struct
    {
        throw new NotImplementedException();
    }

    public void SetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
    {
        throw new NotImplementedException();
    }

    public static Texture2D FromStream(GraphicsDevice graphicsDevice, Stream stream)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
