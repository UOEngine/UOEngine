namespace Microsoft.Xna.Framework.Graphics;

public class GraphicsDevice
{
    public Color BlendFactor;
    public BlendState BlendState;
    public RasterizerState RasterizerState;
    public DepthStencilState DepthStencilState;
    public SamplerStateCollection SamplerStates;

    public void SetVertexBuffer(VertexBuffer vertexBuffer)
    {
        throw new NotImplementedException();
    }

    public IndexBuffer Indices
    {
        get;
        set;
    }

    public Rectangle ScissorRectangle;

    public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int primitiveCount)
    {
        throw new NotImplementedException();
    }
}
