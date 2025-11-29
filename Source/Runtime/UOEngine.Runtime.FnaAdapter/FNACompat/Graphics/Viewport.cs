using System.Numerics;
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
        throw new NotImplementedException();
    }

    public Vector3 Unproject(Vector3 source, Matrix projection, Matrix view, Matrix world)
    {
        throw new NotImplementedException();
    }

    public Vector3 Project(Vector3 source, Matrix projection, Matrix view, Matrix world)
    {
        throw new NotImplementedException();
    }
}
