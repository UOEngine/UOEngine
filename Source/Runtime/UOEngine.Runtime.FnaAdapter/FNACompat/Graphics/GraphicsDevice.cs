using Microsoft.Extensions.DependencyInjection;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.Renderer;
using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

internal sealed class DeviceState<TFnaState, TRhiState>
{
    public TFnaState Value
    {
        get => _value;
        set
        {
            if (ReferenceEquals(_value, value) || Equals(_value, value))
            {
                return;
            }

            _value = value;

            // Clear as dirty, pull from cache later.
            _last = default;
        }
    }

    public TRhiState Get(Func<TFnaState, int> hash, Func<TFnaState, TRhiState> build)
    {
        if (_last != null) return _last;

        var key = hash(_value);

        if (_cache.TryGetValue(key, out var cached))
            return _last = cached;

        return _last = _cache[key] = build(_value);
    }

    private TFnaState _value;
    private readonly Dictionary<int, TRhiState> _cache = [];
    private TRhiState? _last;

    public DeviceState(TFnaState initial) => _value = initial;
}

public class GraphicsDevice
{
    // Per XNA4 General Spec
    internal const int MAX_TEXTURE_SAMPLERS = 16;

    // Per XNA4 HiDef Spec
    internal const int MAX_VERTEX_ATTRIBUTES = 16;
    internal const int MAX_RENDERTARGET_BINDINGS = 4;
    internal const int MAX_VERTEXTEXTURE_SAMPLERS = 4;

    public Color BlendFactor
    {
        get => _blendFactor;
        set
        {
            if (value == _blendFactor) return;
            _blendFactor = value;

        }
    }

    public BlendState BlendState
    {
        get => _blendState.Value;
        set => _blendState.Value = value ?? BlendState.Opaque;
    }

    public RasterizerState RasterizerState
    {
        get => _rasteriserState.Value;
        set => _rasteriserState.Value = value ?? RasterizerState.CullCounterClockwise;
    }

    public DepthStencilState DepthStencilState
    {
        get => _depthStencilState.Value;
        set => _depthStencilState.Value = value ?? DepthStencilState.None;
    }
    public SamplerStateCollection SamplerStates { get; private set; }

    public GraphicsAdapter Adapter;
    public PresentationParameters PresentationParameters;

    public IndexBuffer Indices
    {
        get => _indices;
        set
        {
            if (ReferenceEquals(_indices, value)) return;
            _indices = value;
            _indicesDirty = true;
        }
    }

    public ShaderInstance ShaderInstance
    {
        get => _shaderInstance;
        set
        {
            if(ReferenceEquals(_shaderInstance, value))
            {
                return;
            }

            _shaderInstance = value;
            _graphicsPipelineDirty = true;
        }
    }

    public Rectangle ScissorRectangle;

    public Viewport Viewport;

    public TextureCollection Textures;

    public readonly IRenderResourceFactory RenderResourceFactory;

    public readonly Remapper EffectRemapper;

    private IRenderContext _renderContext;

    private RenderTarget2D? _renderTarget;
    private Color _clearColour;

    private VertexBuffer _vertexBuffer;
    private bool _vertexBufferDirty = false;

    private readonly bool[] _modifiedSamplers = new bool[MAX_TEXTURE_SAMPLERS];
    private readonly bool[] _modifiedVertexSamplers = new bool[MAX_VERTEXTEXTURE_SAMPLERS];

    private IndexBuffer _indices;
    private bool _indicesDirty = true;

    private Color _blendFactor = Color.White;
    private readonly DeviceState<BlendState, RhiBlendState> _blendState = new(BlendState.Opaque);
    private readonly DeviceState<RasterizerState, RhiRasteriserState> _rasteriserState = new(RasterizerState.CullCounterClockwise);
    private readonly DeviceState<DepthStencilState, RhiDepthStencilState> _depthStencilState = new(DepthStencilState.None);

    private ShaderInstance _shaderInstance;
    private bool _graphicsPipelineDirty = false;

    private RhiRasteriserState GetRasteriserState() => _rasteriserState.Get((state) =>
    {
        return 0;
    }, (state) =>
    {
        return new RhiRasteriserState();
    });

    private RhiBlendState GetBlendState() => _blendState.Get((state) =>
    {
        return 0;
    }, 
    (state) =>
    {
        return new RhiBlendState();
    });

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
        _vertexBufferDirty = true;
    }

    public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int primitiveCount)
    {
        BindVertexBufferIfNeeded();
        BindIndexBufferIfNeeded();
        BindGraphicsPipelineIfNeeded();

        throw new NotImplementedException();

        //_renderContext.DrawIndexedPrimitives()
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

    private void BindIndexBufferIfNeeded()
    {
        if (!_indicesDirty || _indices == null)
        {
            return;
        }

        _renderContext.IndexBuffer = _indices.RhiIndexBuffer;

        _indicesDirty = false;
    }

    private void BindGraphicsPipelineIfNeeded()
    {
        if(_graphicsPipelineDirty == false)
        {
            return;
        }

        //_renderContext.GraphicsPipline = ;

        _graphicsPipelineDirty = false;
    }

    private void BindVertexBufferIfNeeded()
    {
        if (_vertexBufferDirty == false)
        {
            return;
        }

        _renderContext.VertexBuffer = _vertexBuffer.RhiVertexBuffer;
        _vertexBufferDirty = false;
    }
}

