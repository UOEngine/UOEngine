// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Numerics;

using UOEngine.Runtime.Application;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.Renderer;
using UOEngine.Runtime.RHI;

namespace UOEngine.Tests.TexturedQuad;

internal class TexturedQuadTest(IServiceProvider serviceProvider) : UOEngineApplication(serviceProvider)
{
    private IndexBuffer _indexBuffer = null!;
    private VertexBuffer<PositionUVVertex> _vertexBuffer = null!;
    private RhiShaderResource _shaderResource = null!;
    private ShaderInstance _shaderInstance = null!;

    private ShaderBindingHandle _worldProjectionHandle;
    private ShaderBindingHandle _textureHandle;

    protected override void OnInitialisationCompleted()
    {
        GetService<RenderSystem>().OnFrameBegin += OnFrameBegin;

        var rendererResourcesFactory = GetService<RendererResourcesFactory>();
        var graphicsFactory = GetService<IRenderResourceFactory>();

        string vertexShader = Path.Combine(UOEPaths.ShadersDir, "Testing/TexturedQuadVS.hlsl");
        string pixelShader = Path.Combine(UOEPaths.ShadersDir, "Testing/TexturedQuadPS.hlsl");

        _shaderResource = graphicsFactory.NewShaderResource();
        _shaderResource.Load(vertexShader, pixelShader);

        _shaderInstance = graphicsFactory.NewShaderInstance(_shaderResource);

        _worldProjectionHandle = _shaderInstance.GetBindingHandleConstantVertex("ProjectionMatrix");
        _textureHandle = _shaderInstance.GetBindingHandleTexturePixel("Texture");

        _indexBuffer = rendererResourcesFactory.NewIndexBuffer(6, "IndexBuffer");

        _indexBuffer.SetData([0, 1, 2, 0, 2, 3]);

        _vertexBuffer = rendererResourcesFactory.NewVertexBuffer<PositionUVVertex>(4);

        _vertexBuffer.Add(new PositionUVVertex
        {
            Position = new Vector3(-0.5f, -0.5f, 0.0f),
            UV = new Vector2(0.0f, 0.0f)
        });

        _vertexBuffer.Add(new PositionUVVertex
        {
            Position = new Vector3(0.5f, -0.5f, 0.0f),
            UV = new Vector2(0.0f, 0.0f)
        });

        _vertexBuffer.Add(new PositionUVVertex
        {
            Position = new Vector3(0.5f, 0.5f, 0.0f),
            UV = new Vector2(0.0f, 0.0f)
        });

        _vertexBuffer.Add(new PositionUVVertex
        {
            Position = new Vector3(-0.5f, 0.5f, 0.0f),
            UV = new Vector2(0.0f, 0.0f)
        });

        _vertexBuffer.SetData();

    }

    public void OnFrameBegin(IRenderContext context)
    {
        context.IndexBuffer = _indexBuffer.RhiBuffer;
        context.VertexBuffer = _vertexBuffer.RhiBuffer;

        var transform = Matrix4x4.CreateTranslation(0.5f, 0.0f, 0.0f);

        _shaderInstance.SetParameter(_worldProjectionHandle, transform);
        _shaderInstance.SetTexture(_textureHandle, GetService<RenderSystem>().GetDefaultTexture(DefaultTextureType.RedCheckerboard));

        context.SetGraphicsPipeline(new RhiGraphicsPipelineDescription
        {
            Shader = _shaderInstance,
            PrimitiveType = RhiPrimitiveType.TriangleList,
            Rasteriser = RhiRasteriserState.CullCounterClockwise,
            BlendState = RhiBlendState.Opaque,
            DepthStencilState = RhiDepthStencilState.None,
            VertexLayout = _vertexBuffer.VertexDefinition
        });

        context.DrawIndexedPrimitives(6, 1, 0, 0, 0);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}
