using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

public class VertexDeclaration
{
    private RhiVertexDefinition _vertexDefinition;

    public VertexDeclaration(params VertexElement[] elements)
    {
        var vertexAttributes = new RhiVertexAttribute[elements.Length];

        for(int i = 0; i < elements.Length; i++)
        {
            ref var element = ref elements[i];

            vertexAttributes[i] = element.Attribute;
        }

        _vertexDefinition = new RhiVertexDefinition(vertexAttributes);
    }

    public VertexDeclaration(int vertexStride, params VertexElement[] elements)
    {
        throw new NotImplementedException();
    }

    internal static VertexDeclaration FromType(Type vertexType)
    {
        if (vertexType == null)
        {
            throw new ArgumentNullException("vertexType", "Cannot be null");
        }

        if (!vertexType.IsValueType)
        {
            throw new ArgumentException("vertexType", "Must be value type");
        }

        IVertexType type = Activator.CreateInstance(vertexType) as IVertexType;
        if (type == null)
        {
            throw new ArgumentException("vertexData does not inherit IVertexType");
        }

        VertexDeclaration vertexDeclaration = type.VertexDeclaration;
        if (vertexDeclaration == null)
        {
            throw new ArgumentException("vertexType's VertexDeclaration cannot be null");
        }

        return vertexDeclaration;
    }
}
