namespace Microsoft.Xna.Framework.Graphics;

public class Texture2D
{
    public Texture2D(
        GraphicsDevice graphicsDevice,
        int width,
        int height,
        bool mipMap,
        SurfaceFormat format
    )
    {
    }

    public void SetData<T>(T[] data) where T : struct
    {
    }

    public void SetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
    {
    }

    public void Dispose()
    {

    }
}
