// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Text;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Renderer;

public class FullscreenPassUtils
{
    private readonly GlobalRenderResources _globalResources;

    public void BlitTexture(IRenderContext context, IRenderTexture texture, RhiRenderTarget? target)
    {
        context.TransitionTextureUsage(texture, RhiRenderTextureUsage.Sampler);

        context.IndexBuffer = _globalResources.ScreenTriangleIndexBuffer.RhiBuffer;
        context.VertexBuffer = null;

        context.SetGraphicsPipeline(new RhiGraphicsPipelineDescription
        {
            BlendState = RhiBlendState.Opaque,
            Shader = _globalResources.BlitTextureShaderInstance,
            DepthStencilState = RhiDepthStencilState.None,
            Rasteriser = RhiRasteriserState.CullCounterClockwise,
            PrimitiveType = RhiPrimitiveType.TriangleList,
            VertexLayout = null
        });

        context.DrawIndexedPrimitives(3, 1, 0, 0, 0);

    }

    internal FullscreenPassUtils(GlobalRenderResources globalResources)
    {
        _globalResources = globalResources;
    }
}
