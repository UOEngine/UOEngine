using System.Numerics;
using Microsoft.Extensions.DependencyInjection;

using UOEngine.Runtime.Core;
using UOEngine.Runtime.Platform;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.Renderer;
using UOEngine.Runtime.RHI;
using UOEngine.Runtime.RHI.Resources;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Ultima.UOAssets;

namespace UOEngine.Editor;

internal class UO3DApplication : IPlugin
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

    private readonly EntityManager _entityManager;
    private readonly RenderSystem _rendererSystem;
    private readonly IRenderResourceFactory _renderFactory;
    private CameraEntity? _camera;
    private readonly UOAssetLoader _assetLoader;
    private MapEntity _map = null!;
    private IRenderTexture _waterTexture = null!;
    private readonly IWindow _window;

    private IRhiIndexBuffer _indexBuffer;

    public UO3DApplication(IServiceProvider serviceProvider)
    {
        _entityManager = serviceProvider.GetRequiredService<EntityManager>();
        _assetLoader = serviceProvider.GetRequiredService<UOAssetLoader>();
        _renderFactory = serviceProvider.GetRequiredService<IRenderResourceFactory>();
        _rendererSystem = serviceProvider.GetRequiredService<RenderSystem>();

        _window = serviceProvider.GetRequiredService<IWindow>();
    }

    public void PostStartup()
    {
        _rendererSystem.OnFrameBegin += OnFrameBegin;

        string vertexShader = @"D:\UODev\UOEngineGithub\Source\Shaders\TexturedQuadVS.hlsl";
        string pixelShader = @"D:\UODev\UOEngineGithub\Source\Shaders\TexturedQuadPS.hlsl";

        _shaderResource = _renderFactory.NewShaderResource();
        _shaderResource.Load(vertexShader, pixelShader);

        _shaderInstance = _renderFactory.NewShaderInstance(_shaderResource);

        _textureBindingHandle = _shaderInstance.GetBindingHandleTexturePixel("Texture");
        _samplerBindingHandle = _shaderInstance.GetBindingHandleSamplerPixel("Sampler");

        _whiteTexture = CreateTestTexture(0xFFFFFFFF, "WhiteTexture");
        _redTexture = CreateTestTexture(0xFF0000FF, "RedTexture");
        _greenTexture = CreateTestTexture(0x00FF00FF, "GreenTexture");

        _checkerboardTexture = CreateCheckerboardTexture(512, 512, Colour.Red, "RedCheckerboard");

        _projectionBinding = _shaderInstance.GetBindingHandleConstantVertex("PerViewData");

        _pipeline = _renderFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription
        {
            Name = "TestPipeline",
            ShaderResource = _shaderResource
        });

        _assetLoader.LoadAllFiles("D:\\Program Files (x86)\\Electronic Arts\\Ultima Online Classic");

        _camera = _entityManager.NewEntity<CameraEntity>();
        _map = _entityManager.NewEntity<MapEntity>();

        _map.Load(_assetLoader.Maps[0]);

        _waterTexture = _renderFactory.CreateTexture(new RhiTextureDescription
        {
            Height = 44,
            Width = 44,
            Name = "Water",
            Usage = RhiRenderTextureUsage.Sampler
        });

        var water = _map.GetChunk(0, 0).Entities[0, 0];

        var bitmap = _assetLoader.GetLand(water.GraphicId);

        bitmap.Texels.CopyTo(_waterTexture.GetTexelsAs<uint>());

        _waterTexture.Upload();

        _indexBuffer = _renderFactory.CreateIndexBuffer(6, "MainIndexBuffer");

        _indexBuffer.SetData([0, 1, 2, 0, 2, 3]);

        _indexBuffer.Upload();
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<UOAssetLoader>();
        services.AddPlugin<FnaAdapterPlugin>();
    }

    public void OnFrameBegin(IRenderContext context)
    {
        //var width = _window.RenderTargetWidth;
        //var height = _window.RenderTargetHeight;

        //Matrix4x4 projection = Matrix4x4.CreateOrthographic(width, height, -1.0f, 1.0f);

        //var mvp = new ModelViewProjection
        //{
        //    Projection = projection,
        //    View = Matrix4x4.Identity
        //};

        //_shaderInstance.SetData(_projectionBinding, mvp);
        //_shaderInstance.SetTexture(_textureBindingHandle, _waterTexture);
        //_shaderInstance.SetSampler(_samplerBindingHandle, new RhiSampler { Filter = RhiSamplerFilter.Point });

        //context.GraphicsPipline = _pipeline;
        //context.ShaderInstance = _shaderInstance;

        //context.IndexBuffer = _indexBuffer;

        //context.DrawIndexedPrimitives(1);
    }

    private IRenderTexture CreateTestTexture(uint colour, string name)
    {
        var texture = _renderFactory.CreateTexture(new RhiTextureDescription
        {
            Width = 22,
            Height = 22,
            Name = name,
            Usage = RhiRenderTextureUsage.Sampler
        });

        Span<uint> white = texture.GetTexelsAs<uint>();

        white.Fill(colour);

        texture.Upload();

        return texture;
    }

    private IRenderTexture CreateCheckerboardTexture(uint width, uint height, in Colour colour, string name)
    {
        var texture = _renderFactory.CreateTexture(new RhiTextureDescription
        {
            Width = width,
            Height = height,
            Name = name,
            Usage = RhiRenderTextureUsage.Sampler
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
