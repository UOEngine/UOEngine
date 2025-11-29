using Microsoft.Extensions.DependencyInjection;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.Renderer;
using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

public class GraphicsDevice
{
    // Per XNA4 General Spec
    internal const int MAX_TEXTURE_SAMPLERS = 16;

    // Per XNA4 HiDef Spec
    internal const int MAX_VERTEX_ATTRIBUTES = 16;
    internal const int MAX_RENDERTARGET_BINDINGS = 4;
    internal const int MAX_VERTEXTEXTURE_SAMPLERS = 4;

    public Color BlendFactor;
    public BlendState BlendState;
    public RasterizerState RasterizerState;
    public DepthStencilState DepthStencilState;
    public SamplerStateCollection SamplerStates { get; private set; }

    public GraphicsAdapter Adapter;
    public PresentationParameters PresentationParameters;

    public IndexBuffer Indices;
    public Rectangle ScissorRectangle;

    public Viewport Viewport;

    public TextureCollection Textures;

    public readonly IRenderResourceFactory RenderResourceFactory;

    public readonly Remapper EffectRemapper;

    private IRenderContext _renderContext;

    private RenderTarget2D? _renderTarget;
    private Color _clearColour;

    private VertexBuffer _vertexBuffer;
    private bool _vertexBuffersUpdated = false;

    private readonly bool[] _modifiedSamplers = new bool[MAX_TEXTURE_SAMPLERS];
    private readonly bool[] _modifiedVertexSamplers = new bool[MAX_VERTEXTEXTURE_SAMPLERS];

    public GraphicsDevice(IServiceProvider serviceProvider)
    {
        RenderResourceFactory = serviceProvider.GetRequiredService<IRenderResourceFactory>();

        // We need to set this each frame and then record into it what is done.
        serviceProvider.GetRequiredService<RenderSystem>().OnFrameBegin += (renderContext) =>
        {
            _renderContext = renderContext;
        };

        SamplerStates = new SamplerStateCollection(1, _modifiedSamplers); 

        Adapter = new GraphicsAdapter();

        EffectRemapper = serviceProvider.GetRequiredService<Remapper>();
    }

    public void SetVertexBuffer(VertexBuffer vertexBuffer)
    {
        _vertexBuffer = vertexBuffer;
        _vertexBuffersUpdated = true;
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
        _clearColour = color;
    }

    public void Clear(ClearOptions options, Color color, float depth, int stencil)
    {
        throw new NotImplementedException();
    }

    public void SetRenderTarget(RenderTarget2D? renderTarget)
    {
        _renderTarget = renderTarget;

    }
}

