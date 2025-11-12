using System.Numerics;

namespace Microsoft.Xna.Framework.Graphics;

public class GraphicsDevice
{
    public Color BlendFactor;
    public BlendState BlendState;
    public RasterizerState RasterizerState;
    public DepthStencilState DepthStencilState;
    public SamplerStateCollection SamplerStates;

    public GraphicsAdapter Adapter;
    public PresentationParameters PresentationParameters;

    public IndexBuffer Indices;
    public Rectangle ScissorRectangle;

    public Viewport Viewport;

    public void SetVertexBuffer(VertexBuffer vertexBuffer)
    {
        throw new NotImplementedException();
    }

    public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int primitiveCount)
    {
        throw new NotImplementedException();
    }

    public void Reset(PresentationParameters presentationParameters)
    {
        throw new NotImplementedException();
    }

    public void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle)
    {

    }

    public void Clear(Color color)
    {
        throw new NotImplementedException();
    }

    public void Clear(ClearOptions options, Color color, float depth, int stencil)
    {
        throw new NotImplementedException();
    }
}

