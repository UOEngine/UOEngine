using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

using UOEngine.Runtime.Core;
using UOEngine.Runtime.FnaAdapter;
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
        get => _fnaBlendState;
        set
        {
            if(value == _fnaBlendState) return;

            _fnaBlendState = value;
            _graphicsPipelineDirty = true;
        }
    }

    public RasterizerState RasterizerState
    {
        get => _rasterizerState;
        set
        {
            if (value == _rasterizerState)
            {
                return;
            }

            _rasterizerState = value;

            _graphicsPipelineDirty = true;
        }
    }

    public DepthStencilState DepthStencilState
    {
        get => _depthStencilState;
        set
        {
            if (value == _depthStencilState)
            {
                return;
            }

            _depthStencilState = value;

            _graphicsPipelineDirty = true;
        }
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

    public TextureCollection Textures { get; private set; }

    public readonly IRenderResourceFactory RenderResourceFactory;

    public readonly Remapper EffectRemapper;

    public IRenderContext RenderContext { get; private set; } = null!;

    private RenderTarget2D? _renderTarget;
    private Color _clearColour;

    private VertexBuffer? _vertexBuffer;
    private bool _vertexBufferDirty = false;

    private readonly bool[] _modifiedSamplers = new bool[MAX_TEXTURE_SAMPLERS];
    private readonly bool[] _modifiedVertexSamplers = new bool[MAX_VERTEXTEXTURE_SAMPLERS];

    private IndexBuffer _indices = null!;
    private bool _indicesDirty = true;

    private Color _blendFactor = Color.White;

    private readonly Dictionary<BlendState, RhiBlendState> _blendStates = [];
    private readonly Dictionary<RasterizerState, RhiRasteriserState> _rasteriserStates = [];
    private readonly Dictionary<DepthStencilState, RhiDepthStencilState> _depthStencilStates = [];
    private readonly Dictionary<SamplerState, RhiSampler> _samplerStates = [];

    private ShaderInstance _shaderInstance = null!;
    private bool _graphicsPipelineDirty = false;

    private BlendState _fnaBlendState = null!;
    private RasterizerState _rasterizerState = null!;
    private DepthStencilState _depthStencilState = null!;

    private List<VertexBuffer> _dynamicVertexBuffers = [];

    public GraphicsDevice(IServiceProvider serviceProvider)
    {
        PresentationParameters = new PresentationParameters();

        RenderResourceFactory = serviceProvider.GetRequiredService<IRenderResourceFactory>();

        SamplerStates = new SamplerStateCollection(MAX_TEXTURE_SAMPLERS, _modifiedSamplers); 

        Adapter = new GraphicsAdapter();

        EffectRemapper = serviceProvider.GetRequiredService<Remapper>();

        Textures = new TextureCollection(MAX_TEXTURE_SAMPLERS, _modifiedSamplers);

        _blendStates.Add(BlendState.AlphaBlend, RhiBlendState.AlphaBlend);
        _blendStates.Add(BlendState.Opaque, RhiBlendState.Opaque);
        _blendStates.Add(BlendState.NonPremultiplied, RhiBlendState.NonPremultiplied);

        _samplerStates.Add(SamplerState.PointClamp, RhiSampler.PointClamp);
        _samplerStates.Add(SamplerState.LinearClamp, RhiSampler.Bilinear);

        _depthStencilStates.Add(DepthStencilState.None, RhiDepthStencilState.None);

        _rasteriserStates.Add(RasterizerState.CullCounterClockwise, RhiRasteriserState.CullCounterClockwise);

    }

    public void SetVertexBuffer(VertexBuffer vertexBuffer)
    {
        _vertexBuffer = vertexBuffer;
        _vertexBufferDirty = true;
    }

    public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int primitiveCount)
    {
        ApplyState();

        int numIndices = primitiveCount * 3;

        RenderContext.DrawIndexedPrimitives((uint)numIndices, 1, (uint)startIndex, (uint)baseVertex, 0);
    }

    public void Reset(PresentationParameters presentationParameters)
    {
        PresentationParameters = presentationParameters;

        Viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

        ScissorRectangle = new Rectangle(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight
);
    }

    public void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle)
    {
        // Empty on purpose. Present is handled by UOEngine.
    }

    public void Clear(Color color)
    {
        _clearColour = color;
    }

    public void Clear(ClearOptions options, Color color, float depth, int stencil)
    {
        UOEDebug.NotImplemented();
    }

    public void SetRenderTarget(RenderTarget2D? renderTarget)
    {
        _renderTarget = renderTarget;

    }

    private void BindIndexBufferIfNeeded()
    {
        RenderContext.IndexBuffer = _indices.RhiIndexBuffer;
    }

    internal void RegisterDynamicVertexBuffer(VertexBuffer vertexBuffer)
    {
        UOEDebug.Assert((DynamicVertexBuffer)vertexBuffer != null);

        _dynamicVertexBuffers.Add(vertexBuffer);
    }

    internal void UnregisterDynamicVertexBuffer(VertexBuffer vertexBuffer)
    {
        UOEDebug.Assert((DynamicVertexBuffer)vertexBuffer != null);

        _dynamicVertexBuffers.Remove(vertexBuffer);
    }

    internal void OnFrameBegin(IRenderContext renderContext)
    {
        foreach (var vertexBuffer in _dynamicVertexBuffers)
        {
            vertexBuffer.Reset();
        }

        RenderContext = renderContext;
    }

    private void BindGraphicsPipelineIfNeeded()
    {
        if(_graphicsPipelineDirty == false)
        {
            return;
        }

        Debug.Assert(_blendStates.ContainsKey(_fnaBlendState));

        RenderContext.SetGraphicsPipeline(new RhiGraphicsPipelineDescription
        {
            Shader = _shaderInstance,
            PrimitiveType = RhiPrimitiveType.TriangleList,
            Rasteriser = GetRasteriserState(),
            BlendState = _blendStates[_fnaBlendState],
            DepthStencilState = GetDepthStencilState(),
            VertexLayout = null
        });

        _graphicsPipelineDirty = false;
    }

    private void BindVertexBufferIfNeeded()
    {
        if (_vertexBufferDirty == false)
        {
            return;
        }

        RenderContext.VertexBuffer = _vertexBuffer?.RhiVertexBuffer!;
        _vertexBufferDirty = false;
    }

    private void ApplyState()
    {
        BindVertexBufferIfNeeded();
        BindIndexBufferIfNeeded();
        BindGraphicsPipelineIfNeeded();

        ApplySamplers();

    }

    private void ApplySamplers()
    {
        for(int i = 0; i < _modifiedSamplers.Length; i++)
        {
            if (_modifiedSamplers[i] == false)
            {
                continue;
            }

            UOEDebug.Assert(_samplerStates.ContainsKey(SamplerStates[i]));

            RenderContext.Sampler = _samplerStates[SamplerStates[i]];

            _modifiedSamplers[i] = false;

            var texture = Textures[i];

            if(texture != null)
            {
                // Note texture may have been set by effect into shader instance.
                var bindingHandle = _shaderInstance.GetBindingHandle(ShaderProgramType.Pixel, RhiShaderInputType.Texture, i);

                // Sometimes texture slots are set on the device but are not a valid index for the shader, so we check.
                if(bindingHandle.IsValid)
                {
                    UOEDebug.Assert(_shaderInstance.BindingHandleIsValid(bindingHandle));

                    _shaderInstance.SetTexture(bindingHandle, texture.RhiTexture);

                    Textures[i] = null;
                }
            }
        }
    }

    private RhiRasteriserState GetRasteriserState()
    {
        RhiRasteriserState state;

        if(_rasteriserStates.TryGetValue(_rasterizerState, out state))
        {
            return state;
        }

        state.FrontFace = RhiFrontFace.CounterClockwise;

        state.FrontFace = (_rasterizerState.CullMode) switch
        {
            CullMode.None                     => RhiFrontFace.CounterClockwise,
            CullMode.CullClockwiseFace        => RhiFrontFace.CounterClockwise,
            CullMode.CullCounterClockwiseFace => RhiFrontFace.Clockwise,
                                            _ => throw new SwitchExpressionException("Could not convert cull mode for rasteriser state")
        };

        state.CullMode = (_rasterizerState.CullMode) switch
        {
            CullMode.None                     => RhiCullMode.Disable,
            CullMode.CullClockwiseFace        => RhiCullMode.Back,
            CullMode.CullCounterClockwiseFace => RhiCullMode.Back,
                                              _ => throw new SwitchExpressionException("Could not convert cull mode for rasteriser state")
        };

        state.FillMode = (_rasterizerState.FillMode) switch
        {
            FillMode.WireFrame => RhiFillMode.Wireframe,
            FillMode.Solid     => RhiFillMode.Solid,
                             _ => throw new SwitchExpressionException("Could not convert fill mode for rasteriser state")
        };

        _rasteriserStates.Add(_rasterizerState, state);

        return state;

    }

    private RhiDepthStencilState GetDepthStencilState()
    {
        RhiDepthStencilState newState;

        if (_depthStencilStates.TryGetValue(_depthStencilState, out newState))
        {
            return newState;
        }

        newState.DepthBufferEnable = _depthStencilState.DepthBufferEnable;
        newState.DepthBufferWriteEnable = _depthStencilState.DepthBufferWriteEnable;
        newState.StencilEnable = _depthStencilState.StencilEnable;

        _depthStencilStates.Add(_depthStencilState, newState);

        return newState;

    }
}

