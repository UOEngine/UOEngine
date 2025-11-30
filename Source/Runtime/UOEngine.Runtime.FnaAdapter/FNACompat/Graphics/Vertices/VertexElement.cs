namespace Microsoft.Xna.Framework.Graphics;

public struct VertexElement
{
    public int Offset;
    public VertexElementFormat Format;
    public VertexElementUsage ElementUsage;
    public int UsageIndex;

    public VertexElement(int offset, VertexElementFormat elementFormat, VertexElementUsage elementUsage, int usageIndex) 
    {
        Offset = offset;
        Format = elementFormat;
        ElementUsage = elementUsage;
        UsageIndex = usageIndex;
    }
}
