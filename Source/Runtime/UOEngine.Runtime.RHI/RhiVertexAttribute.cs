using System.Diagnostics;

namespace UOEngine.Runtime.RHI;

[DebuggerDisplay("{Offset}, {Type}")]
public class RhiVertexAttribute
{
    public readonly RhiVertexAttributeType Type;
    public readonly RhiVertexAttributeFormat Format;
    public readonly uint Offset;

    public RhiVertexAttribute(RhiVertexAttributeType type, RhiVertexAttributeFormat format, uint offset)
    {
        Type = type;
        Format = format;
        Offset = offset;
    }
}
