namespace Microsoft.Xna.Framework.Graphics;

public struct VertexElement
{
    public VertexElement(
    int offset,
    VertexElementFormat elementFormat,
    VertexElementUsage elementUsage,
    int usageIndex
) : this()
    {
        Offset = offset;
        UsageIndex = usageIndex;
        VertexElementFormat = elementFormat;
        VertexElementUsage = elementUsage;
    }
}
