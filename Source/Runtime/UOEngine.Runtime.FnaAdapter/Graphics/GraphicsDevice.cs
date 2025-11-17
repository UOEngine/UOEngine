using Microsoft.Extensions.DependencyInjection;
using UOEngine.Runtime.Renderer;
using UOEngine.Runtime.RHI;

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

    public TextureCollection Textures;

    public readonly IRenderResourceFactory RenderResourceFactory;

    private IRenderContext _renderContext;

    public GraphicsDevice(IServiceProvider serviceProvider)
    {
        RenderResourceFactory = serviceProvider.GetRequiredService<IRenderResourceFactory>();

        // We need to set this each frame and then record into it what is done.
        serviceProvider.GetRequiredService<RenderSystem>().OnFrameBegin += (renderContext) =>
        {
            _renderContext = renderContext;
        };

        Adapter = new GraphicsAdapter();
    }

    public void SetVertexBuffer(VertexBuffer vertexBuffer)
    {
        throw new NotImplementedException();
    }

    public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int primitiveCount)
    {
        throw new NotImplementedException();
        //_renderContext.DrawIndexedPrimitives();
    }

    public void Reset(PresentationParameters presentationParameters)
    {
        throw new NotImplementedException();
    }

    public void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle)
    {
        throw new NotImplementedException();
    }

    public void Clear(Color color)
    {
        throw new NotImplementedException();
    }

    public void Clear(ClearOptions options, Color color, float depth, int stencil)
    {
        throw new NotImplementedException();
    }

    public void SetRenderTarget(RenderTarget2D renderTarget)
    {
        throw new NotImplementedException();

    }
}

