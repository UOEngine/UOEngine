namespace UOEngine.Runtime.RHI;

public record struct RhiGraphicsPipelineDescription
{
    public required ShaderInstance Shader { get; init; }
    public required RhiPrimitiveType PrimitiveType { get; init; }
    public required RhiRasteriserState Rasteriser { get; init; }
    public required RhiBlendState BlendState { get; init; }
    public required RhiDepthStencilState DepthStencilState { get; init; }
    public required RhiVertexDefinition? VertexLayout { get; init; }
    //public required RhiRenderTargetFormat ColorFormat { get; init; }
    //public required RhiDepthFormat DepthFormat { get; init; }
}