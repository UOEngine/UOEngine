using UOEngine.Runtime.Core;
using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

public struct Viewport
{
    public int Width => _rhiViewport.Width;
    public int Height => _rhiViewport.Height;   

    private RhiViewport _rhiViewport;

    public Viewport(int x, int y, int width, int height)
    {
        _rhiViewport = new RhiViewport(x, y, width, height);
    }

    public Viewport(Rectangle bounds)
    {
        UOEDebug.NotImplemented();
    }

    public Vector3 Unproject(Vector3 source, Matrix projection, Matrix view, Matrix world)
    {
        // Todo: fix me!
        return Vector3.One;
    }

    public Vector3 Project(Vector3 source, Matrix projection, Matrix view, Matrix world)
    {
        UOEDebug.NotImplemented();

        return Vector3.Zero;
    }
}
