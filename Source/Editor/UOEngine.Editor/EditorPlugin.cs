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
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

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
    private readonly RendererResourcesFactory rendererResourcesFactory;
    private CameraEntity? _camera;
    private readonly UOAssetLoader _assetLoader;
    private MapEntity _map = null!;
    private IRenderTexture _waterTexture = null!;
    private readonly IWindow _window;

    private IndexBuffer _indexBuffer = null!;
    private VertexBuffer _vertexBuffer = null!;

    public UO3DApplication(IServiceProvider serviceProvider)
    {
        _entityManager = serviceProvider.GetRequiredService<EntityManager>();
        _assetLoader = serviceProvider.GetRequiredService<UOAssetLoader>();
        _renderFactory = serviceProvider.GetRequiredService<IRenderResourceFactory>();
        rendererResourcesFactory = serviceProvider.GetRequiredService<RendererResourcesFactory>();
        _rendererSystem = serviceProvider.GetRequiredService<RenderSystem>();

        _window = serviceProvider.GetRequiredService<IWindow>();
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Vertex
    {
        public Vector3 Position;
        public Colour Colour;
    }

    public void PostStartup()
    {
        _rendererSystem.OnFrameBegin += OnFrameBegin;

        string vertexShader = Path.Combine(UOEPaths.ShadersDir, "Testing/TriangleVS.hlsl");
        string pixelShader = Path.Combine(UOEPaths.ShadersDir, "Testing/TrianglePS.hlsl");

        _shaderResource = _renderFactory.NewShaderResource();
        _shaderResource.Load(vertexShader, pixelShader);

        _shaderInstance = _renderFactory.NewShaderInstance(_shaderResource);

        //_textureBindingHandle = _shaderInstance.GetBindingHandleTexturePixel("Texture");
        //_samplerBindingHandle = _shaderInstance.GetBindingHandleSamplerPixel("Sampler");

        //_whiteTexture = CreateTestTexture(0xFFFFFFFF, "WhiteTexture");
        //_redTexture = CreateTestTexture(0xFF0000FF, "RedTexture");
        //_greenTexture = CreateTestTexture(0x00FF00FF, "GreenTexture");

        _checkerboardTexture = CreateCheckerboardTexture(512, 512, Colour.Red, "RedCheckerboard");

        _indexBuffer = rendererResourcesFactory.NewIndexBuffer(3, "IndexBuffer");

        _indexBuffer.SetData([0, 1, 2]);

        _vertexBuffer = rendererResourcesFactory.NewVertexBuffer<Vertex>(3);

        var v0 = new Vertex
        {
            Position = Vector3.Zero,
            Colour = Colour.Red,
        };

        var v1 = new Vertex
        {
            Position = Vector3.Zero,
            Colour = Colour.Green,
        };

        var v2 = new Vertex
        {
            Position = Vector3.Zero,
            Colour = Colour.Blue,
        };

        _vertexBuffer.SetData([v0, v1, v2]);
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<UOAssetLoader>();
        services.AddPlugin<FnaAdapterPlugin>();
    }

    public void OnFrameBegin(IRenderContext context)
    {
        //_shaderInstance.SetTexture(_textureBindingHandle, _checkerboardTexture);

        context.IndexBuffer = _indexBuffer.RhiBuffer;
        context.VertexBuffer = _vertexBuffer.RhiBuffer;

        context.DrawIndexedPrimitives(3, 1, 0, 0, 0);
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
