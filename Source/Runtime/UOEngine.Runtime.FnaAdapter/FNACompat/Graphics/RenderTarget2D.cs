using UOEngine.Runtime.Core;
using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

public class RenderTarget2D: Texture2D
{
    public int LevelCount { get;}

    public DepthFormat DepthStencilFormat;
    public SurfaceFormat Format;

    private RhiRenderTarget _rhiRenderTarget;

    public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat)
        : base(graphicsDevice, width, height, mipMap, preferredFormat, RhiRenderTextureUsage.ColourTarget)
    {
        _rhiRenderTarget = new RhiRenderTarget();

        _rhiRenderTarget.Setup(RhiTexture);
    }

    public void GetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
    {
        // Clear to empty for now.
        for(int i = 0; i < data.Length; i++)
        {
            data[i] = default;
        }
    }

    public void SaveAsPng(FileStream fileStream, int width, int height)
    {
        UOEDebug.NotImplemented();
    }

    public void SaveAsJpeg(FileStream fileStream, int width, int height)
    {
        UOEDebug.NotImplemented();
    }

    public void Dispose()
    {
        // Todo: Do we need to dispose this?
        //UOEDebug.NotImplemented();
    }
}
