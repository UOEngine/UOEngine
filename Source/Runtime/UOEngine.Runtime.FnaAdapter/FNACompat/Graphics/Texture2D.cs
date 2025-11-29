using StbImageSharp;
using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

public class Texture2D: Texture
{
    public int Width => (int)RhiTexture.Width;
    public int Height => (int)RhiTexture.Height;

    public Rectangle Bounds
    {
        get
        {
            return new Rectangle(0, 0, Width, Height);
        }
    }

    protected readonly IRenderTexture RhiTexture;

    public Texture2D(
        GraphicsDevice graphicsDevice,
        int width,
        int height,
        bool mipMap,
        SurfaceFormat format
    ): this(graphicsDevice, width, height, mipMap, format, RhiRenderTextureUsage.Sampler)
    {
    }

    public Texture2D(GraphicsDevice graphicsDevice, int width, int height)
        : this(graphicsDevice, width, height, false, SurfaceFormat.Color, RhiRenderTextureUsage.Sampler)
    {
    }

    protected Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat format, RhiRenderTextureUsage usage)
    {
        RhiTexture = graphicsDevice.RenderResourceFactory.CreateTexture(new RhiTextureDescription
        {
            Width = (uint)width,
            Height = (uint)height,
            Usage = usage
        });
    }

    //public void SetData<T>(T[] data) where T : struct
    //{

    //}

    public void SetData<T>(T[] data) where T : unmanaged
    {
        var texels = RhiTexture.GetTexelsAs<T>();

        data.CopyTo(texels);

        RhiTexture.Upload();
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
