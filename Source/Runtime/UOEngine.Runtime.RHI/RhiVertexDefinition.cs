namespace UOEngine.Runtime.RHI;

public class RhiVertexDefinition
{
    public readonly uint Stride;

    public readonly RhiVertexAttribute[] Attributes;
    public RhiVertexDefinition(RhiVertexAttribute[] attributes)
    {
        Attributes = attributes;

        foreach(var attribute in Attributes)
        {
            Stride += SizeOfFormat(attribute.Format);
        }
    }
    private static uint SizeOfFormat(RhiVertexAttributeFormat f) => f switch
    {
        RhiVertexAttributeFormat.Vector2        => 8,
        RhiVertexAttributeFormat.Vector3        => 12,
        RhiVertexAttributeFormat.Vector4        => 16,
        RhiVertexAttributeFormat.R8G8B8A8_UNorm => 4,
        _ => throw new NotSupportedException()
    };
}
