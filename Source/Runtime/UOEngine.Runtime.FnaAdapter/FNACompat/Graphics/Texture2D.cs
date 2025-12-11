using System.Runtime.CompilerServices;
using StbImageSharp;

using UOEngine.Runtime.Core;
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

    public Texture2D(
        GraphicsDevice graphicsDevice,
        int width,
        int height,
        bool mipMap,
        SurfaceFormat format
    ): this(graphicsDevice, width, height, mipMap, format, RhiRenderTextureUsage.Sampler)
    {
        Format = format;
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

    public void SetData<T>(T[] data) where T : unmanaged
    {
        var texels = RhiTexture.GetTexelsAs<T>();

        data.CopyTo(texels);

        RhiTexture.Upload();
    }

    public void SetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : unmanaged
    {
        int x, y, w, h;

        if (rect.HasValue)
        {
            x = rect.Value.X;
            y = rect.Value.Y;
            w = rect.Value.Width;
            h = rect.Value.Height;
        }
        else
        {
            x = 0;
            y = 0;
            w = Math.Max(Width >> level, 1);
            h = Math.Max(Height >> level, 1);
        }

        int elementSize = Unsafe.SizeOf<T>();

        var src = data.AsSpan();
        var dst = RhiTexture.GetTexelsAs<T>(); 

        int dstRowPitch = (int)RhiTexture.Width >> level; // pixels per row at this mip
        int srcRowPitch = w * elementSize;

        for (int texelY = 0; texelY < h; texelY++)
        {
            var srcRow = src.Slice(y * srcRowPitch, w);
            var dstRow = dst.Slice((y + texelY) * dstRowPitch + x, w);

            srcRow.CopyTo(dstRow);
        }

        RhiTexture.Upload((uint)x, (uint)y, (uint)w, (uint)h);
    }

    public static Texture2D FromStream(GraphicsDevice graphicsDevice, Stream stream)
    {
        if (stream.CanSeek && stream.Position == stream.Length)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        var data = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        // Mimic what FNA3D does in its .c file to read an image.
        for(int i = 0; i < data.Data.Length; i += 4)
        {
            if (data.Data[i + 3] == 0)
            {
                data.Data[i] = 0;
                data.Data[i+1] = 0;
                data.Data[i+2] = 0;
            }
        }

        var texture = new Texture2D(graphicsDevice, data.Width, data.Height);

        texture.SetData(data.Data);

        return texture;
    }

    public void SetDataPointerEXT(int level, Rectangle? rect, IntPtr data, int dataLength)
    {
        int x, y, w, h;

        if (rect.HasValue)
        {
            x = rect.Value.X;
            y = rect.Value.Y;
            w = rect.Value.Width;
            h = rect.Value.Height;

            UOEDebug.NotImplemented();
        }
        else
        {
            x = 0;
            y = 0;
            w = Math.Max(Width >> level, 1);
            h = Math.Max(Height >> level, 1);

            var dst = RhiTexture.GetTexelsAs<byte>();

            unsafe
            {
                var src = new Span<byte>((void*)data, dataLength);

                src.CopyTo(dst);
            }
        }

        RhiTexture.Upload((uint)x, (uint)y, (uint)w, (uint)h);
    }

    public void Dispose()
    {
        UOEDebug.NotImplemented();
    }
}
