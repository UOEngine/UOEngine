// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.RHI;

public record struct RhiGraphicsPipelineDescription
{
    public required ShaderInstance Shader { get; init; }
    public required RhiPrimitiveType PrimitiveType { get; init; }
    public required RhiRasteriserState Rasteriser { get; init; }
    public required RhiBlendState BlendState { get; init; }
    public required RhiDepthStencilState DepthStencilState { get; init; }
    public required RhiVertexDefinition? VertexLayout { get; set; }
    //public required RhiRenderTargetFormat ColorFormat { get; init; }
    //public required RhiDepthFormat DepthFormat { get; init; }
}