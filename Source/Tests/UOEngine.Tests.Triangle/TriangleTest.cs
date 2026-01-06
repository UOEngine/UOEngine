// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Numerics;

using UOEngine.Runtime.Application;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.Renderer;
using UOEngine.Runtime.RHI;

namespace UOEngine.Tests.Triangle;

internal class TriangleTest(IServiceProvider serviceProvider) : UOEngineApplication(serviceProvider)
{
    private IndexBuffer _indexBuffer = null!;
    private VertexBuffer<PositionAndColourVertex> _vertexBuffer = null!;
    private RhiShaderResource _shaderResource = null!;
    private ShaderInstance _shaderInstance = null!;

    protected override void OnInitialisationCompleted()
    {
        GetService<RenderSystem>().OnFrameBegin += OnFrameBegin;

        var rendererResourcesFactory = GetService<RendererResourcesFactory>();
        var graphicsFactory = GetService<IRenderResourceFactory>();

        string vertexShader = Path.Combine(UOEPaths.ShadersDir, "Testing/TriangleVS.hlsl");
        string pixelShader = Path.Combine(UOEPaths.ShadersDir, "Testing/TrianglePS.hlsl");

        _shaderResource = graphicsFactory.NewShaderResource();
        _shaderResource.Load(vertexShader, pixelShader);

        _shaderInstance = graphicsFactory.NewShaderInstance(_shaderResource);

        _indexBuffer = rendererResourcesFactory.NewIndexBuffer(3, "IndexBuffer");

        _indexBuffer.SetData([0, 1, 2]);

        _vertexBuffer = rendererResourcesFactory.NewVertexBuffer<PositionAndColourVertex>(3);

        var v0 = new PositionAndColourVertex
        {
            Position = Vector3.Zero,
            Colour = Colour.Red.ToUint32(),
        };

        var v1 = new PositionAndColourVertex
        {
            Position = new Vector3(0.5f, 0.0f, 0.0f),
            Colour = Colour.Green.ToUint32(),
        };

        var v2 = new PositionAndColourVertex
        {
            Position = new Vector3(0.0f, 1.0f, 0.0f),
            Colour = Colour.Blue.ToUint32(),
        };

        _vertexBuffer.SetData([v0, v1, v2]);
    }

    public void OnFrameBegin(IRenderContext context)
    {
        context.IndexBuffer = _indexBuffer.RhiBuffer;
        context.VertexBuffer = _vertexBuffer.RhiBuffer;

        context.SetGraphicsPipeline(new RhiGraphicsPipelineDescription
        {
            Shader = _shaderInstance,
            PrimitiveType = RhiPrimitiveType.TriangleList,
            Rasteriser = RhiRasteriserState.CullCounterClockwise,
            BlendState = RhiBlendState.Opaque,
            DepthStencilState = RhiDepthStencilState.None,
            VertexLayout = PositionAndColourVertex.Layout
        });

        context.DrawIndexedPrimitives(3, 1, 0, 0, 0);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}
