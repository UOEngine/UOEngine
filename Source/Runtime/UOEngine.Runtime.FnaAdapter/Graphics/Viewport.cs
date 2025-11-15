using System.Numerics;

namespace Microsoft.Xna.Framework.Graphics;

public struct Viewport
{
    public Viewport(int x, int y, int width, int height)
    {
        throw new NotImplementedException();
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
