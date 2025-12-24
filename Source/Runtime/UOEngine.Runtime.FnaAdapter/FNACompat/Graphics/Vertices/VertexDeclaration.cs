using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

public class VertexDeclaration
{
    internal RhiVertexDefinition RhiVertexDefinition;

    public VertexDeclaration(params VertexElement[] elements)
    {
        var vertexAttributes = new RhiVertexAttribute[elements.Length];

        for(int i = 0; i < elements.Length; i++)
        {
            ref var element = ref elements[i];

            vertexAttributes[i] = new RhiVertexAttribute(MapUsage(element.ElementUsage), MapFormat(element.Format), (uint)element.Offset);
        }

        RhiVertexDefinition = new RhiVertexDefinition(vertexAttributes);
    }

    public VertexDeclaration(int vertexStride, params VertexElement[] elements)
        : this(elements)
    {
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

        IVertexType? type = Activator.CreateInstance(vertexType) as IVertexType;

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

    private static RhiVertexAttributeFormat MapFormat(VertexElementFormat f) => f switch
    {
        VertexElementFormat.Vector2 => RhiVertexAttributeFormat.Vector2,
        VertexElementFormat.Vector3 => RhiVertexAttributeFormat.Vector3,
        VertexElementFormat.Vector4 => RhiVertexAttributeFormat.Vector4,
        VertexElementFormat.Color   => RhiVertexAttributeFormat.R8G8B8A8_UNorm,
                                  _ => throw new NotSupportedException($"MapFormat: VertexElementFormat {f} not supported")
    };

    private static RhiVertexAttributeType MapUsage(VertexElementUsage usage) => usage switch
    {
        VertexElementUsage.Position          => RhiVertexAttributeType.Position,
        VertexElementUsage.Color             => RhiVertexAttributeType.Colour,
        VertexElementUsage.TextureCoordinate => RhiVertexAttributeType.TextureCoordinate,
                                           _ => throw new NotSupportedException($"MapUsage: VertexElementUsage {usage} not supported")
    };
}
