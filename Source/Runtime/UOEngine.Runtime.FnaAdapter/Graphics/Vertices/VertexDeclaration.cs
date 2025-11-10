using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics;

public class VertexDeclaration
{
//    public VertexDeclaration(
//    params VertexElement[] elements
//) : this(GetVertexStride(elements), elements)
//    {
//    }

    public VertexDeclaration(
        int vertexStride,
        params VertexElement[] elements
    )
    {
    }
}
