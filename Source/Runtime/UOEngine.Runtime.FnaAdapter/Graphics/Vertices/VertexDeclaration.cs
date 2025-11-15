using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics;

public class VertexDeclaration
{
    public VertexDeclaration(params VertexElement[] elements)
    {
        throw new NotImplementedException();
    }

    public VertexDeclaration(int vertexStride, params VertexElement[] elements)
    {
        throw new NotImplementedException();
    }

    internal static VertexDeclaration FromType(Type vertexType)
    {
        throw new NotImplementedException();
    }
}
