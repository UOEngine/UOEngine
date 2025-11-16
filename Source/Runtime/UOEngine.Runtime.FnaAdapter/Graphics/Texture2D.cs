using StbImageSharp;
using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

public class Texture2D: Texture
{
    public int Width => (int)_rhiTexture.Width;
    public int Height => (int)_rhiTexture.Height;

    public Rectangle Bounds
    {
        get
        {
            return new Rectangle(0, 0, Width, Height);
        }
    }

    private readonly IRenderTexture _rhiTexture;

    public Texture2D(
        GraphicsDevice graphicsDevice,
        int width,
        int height,
        bool mipMap,
        SurfaceFormat format
    )
    {
        //_rhiTexture = graphicsDevice.RenderResourceFactory.CreateTexture();

        throw new NotImplementedException();
    }

    public Texture2D(GraphicsDevice graphicsDevice, int width, int height)
    {
        _rhiTexture = graphicsDevice.RenderResourceFactory.CreateTexture(new RenderTextureDescription
        {
            Width = (uint)width,
            Height = (uint)height,
            Usage = RenderTextureUsage.Sampler
        });
    }

    //public void SetData<T>(T[] data) where T : struct
    //{

    //}

    public void SetData<T>(T[] data) where T : unmanaged
    {
        var texels = _rhiTexture.GetTexelsAs<T>();

        data.CopyTo(texels);

        _rhiTexture.Upload();
    }

    public void SetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
    {
        throw new NotImplementedException();
    }

    public static Texture2D FromStream(GraphicsDevice graphicsDevice, Stream stream)
    {
        if (stream.CanSeek && stream.Position == stream.Length)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        var data = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        var texture = new Texture2D(graphicsDevice, data.Width, data.Height);

        texture.SetData(data.Data);

        return texture;
    }

    public void SetDataPointerEXT(int level, Rectangle? rect, IntPtr data, int dataLength)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
