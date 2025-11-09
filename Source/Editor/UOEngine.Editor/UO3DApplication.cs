using System.Numerics;

using UOEngine.Runtime.Core;
using UOEngine.Runtime.RHI;
using UOEngine.Runtime.RHI.Resources;

namespace UO3D;

internal class UO3DApplication: Application
{
    private RhiShaderResource _shaderResource = null!;
    private ShaderInstance _shaderInstance = null!;
    private ShaderBindingHandle _projectionBinding;
    private IGraphicsPipeline _pipeline = null!;
    private IRenderTexture _whiteTexture = null!;
    private IRenderTexture _redTexture = null!;
    private IRenderTexture _greenTexture = null!;
    private IRenderTexture _checkerboardTexture = null!;

    private ShaderBindingHandle _textureBindingHandle = ShaderBindingHandle.Invalid;
    private ShaderBindingHandle _samplerBindingHandle = ShaderBindingHandle.Invalid;

    protected override void Initialise()
    {
        var renderFactory = GetService<IRenderResourceFactory>();

        string vertexShader = @"D:\UODev\Work\UO3D\Source\Shaders\TexturedQuadVS.hlsl";
        string pixelShader = @"D:\UODev\Work\UO3D\Source\Shaders\TexturedQuadPS.hlsl";

        _shaderResource = renderFactory.NewShaderResource();
        _shaderResource.Load(vertexShader, pixelShader);

        _shaderInstance = renderFactory.NewShaderInstance(_shaderResource);

        _textureBindingHandle = _shaderInstance.GetBindingHandleTexturePixel("Texture");
        _samplerBindingHandle = _shaderInstance.GetBindingHandleSamplerPixel("Sampler");

        _whiteTexture = CreateTestTexture(0xFFFFFFFF, "WhiteTexture");
        _redTexture = CreateTestTexture(0xFF0000FF, "RedTexture");
        _greenTexture = CreateTestTexture(0x00FF00FF, "GreenTexture");

        _checkerboardTexture = CreateCheckerboardTexture(512, 512, Colour.Red, "RedCheckerboard");

        _projectionBinding = _shaderInstance.GetBindingHandleConstantVertex("PerViewData");

        _pipeline = renderFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription
        {
            Name = "TestPipeline",
            ShaderResource = _shaderResource
        });
    }

    protected override void BeginDraw(IRenderContext context)
    {
        Matrix4x4 projection = Matrix4x4.Identity;

       var mvp = new ModelViewProjection
        {
            Projection = Matrix4x4.Identity,
            View = Matrix4x4.CreateTranslation(-0.5f, -0.5f, 0.0f)
        };

        _shaderInstance.SetData(_projectionBinding, mvp);
        _shaderInstance.SetTexture(_textureBindingHandle, _checkerboardTexture);
        _shaderInstance.SetSampler(_samplerBindingHandle, new RhiSampler { Filter = SamplerFilter.Point});

        context.GraphicsPipline = _pipeline;
        context.ShaderInstance = _shaderInstance;

        context.DrawIndexedPrimitives(1);
    }

    private IRenderTexture CreateTestTexture(uint colour, string name)
    {
        var renderFactory = GetService<IRenderResourceFactory>();

        var texture = renderFactory.CreateTexture(new RenderTextureDescription
        {
            Width = 22,
            Height = 22,
            Name = name,
            Usage = RenderTextureUsage.Sampler
        });

        Span<uint> white = texture.GetTexelsAs<uint>();

        white.Fill(colour);

        texture.Upload();

        return texture;
    }

    private IRenderTexture CreateCheckerboardTexture(uint width, uint height, in Colour colour, string name)
    {
        var renderFactory = GetService<IRenderResourceFactory>();

        var texture = renderFactory.CreateTexture(new RenderTextureDescription
        {
            Width = width,
            Height = height,
            Name = name,
            Usage = RenderTextureUsage.Sampler
        });

        Span<Colour> texels = texture.GetTexelsAs<Colour>();

        int checkSize = 16;

        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < height; x++)
            {
                bool isWhite = ((x / checkSize) + (y / checkSize)) % 2 == 0;

                int i = y * (int)width + x;

                if (isWhite)
                {
                    texels[i] = Colour.White;
                }
                else
                {
                    texels[i] = colour;
                }
            }
        }

        texture.Upload();

        return texture;
    }

}
