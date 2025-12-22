namespace UOEngine.Runtime.RHI;

[Flags]
public enum RhiVertexBufferFlags
{
    None = 0,
    Dynamic
}

public struct RhiVertexBufferDescription
{
    public uint VertexCount;
    public RhiVertexDefinition AttributesDefinition;
    public uint Stride;
    public RhiVertexBufferFlags Flags;
}
