namespace UOEngine.Runtime.RHI;

public class RhiVertexAttribute
{
    public readonly RhiVertexAttributeType Type;
    public readonly uint Offset;

    public RhiVertexAttribute(RhiVertexAttributeType type, uint offset)
    {
        Type = type;
        Offset = offset;
    }
}
