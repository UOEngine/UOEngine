// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Microsoft.Extensions.DependencyInjection;

using UOEngine.Runtime.Core;
using UOEngine.Runtime.Platform;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.Renderer;
using UOEngine.Runtime.RHI;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Ultima.UOAssets;

namespace UOEngine.Editor;

[PluginEntry]
[PluginLoadingPhase(PluginLoadingPhase.Default)]
internal class UO3DApplication : IPlugin
{
    private RhiShaderResource _shaderResource = null!;
    private ShaderInstance _shaderInstance = null!;
    private ShaderBindingHandle _projectionBinding;
    private IRhiGraphicsPipeline _pipeline = null!;
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

        string vertexShader = Path.Combine(UOEPaths.ShadersDir, "TexturedQuadVS.hlsl");
        string pixelShader = Path.Combine(UOEPaths.ShadersDir, "TexturedQuadPS.hlsl");

        _shaderResource = _renderFactory.NewShaderResource();
        _shaderResource.Load(vertexShader, pixelShader);

        _shaderInstance = _renderFactory.NewShaderInstance(_shaderResource);

        _textureBindingHandle = _shaderInstance.GetBindingHandleTexturePixel("Texture");
        _samplerBindingHandle = _shaderInstance.GetBindingHandleSamplerPixel("Sampler");

        //_whiteTexture = CreateTestTexture(0xFFFFFFFF, "WhiteTexture");
        //_redTexture = CreateTestTexture(0xFF0000FF, "RedTexture");
        //_greenTexture = CreateTestTexture(0x00FF00FF, "GreenTexture");

        _checkerboardTexture = CreateCheckerboardTexture(512, 512, Colour.Red, "RedCheckerboard");

        var indexBuffer = _renderFactory.CreateIndexBuffer(6, "IndexBuffer");
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<UOAssetLoader>();
        services.AddPlugin<FnaAdapterPlugin>();
    }

    public void OnFrameBegin(IRenderContext context)
    {
        _shaderInstance.SetTexture(_textureBindingHandle, _checkerboardTexture);
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
