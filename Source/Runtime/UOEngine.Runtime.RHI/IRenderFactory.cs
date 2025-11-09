using UOEngine.Runtime.RHI.Resources;

namespace UOEngine.Runtime.RHI;

public interface IRenderResourceFactory
{
    public RhiShaderResource NewShaderResource();
    public ShaderInstance NewShaderInstance(RhiShaderResource shaderResource);

    public IRenderTexture CreateTexture(in RenderTextureDescription description);

    public IGraphicsPipeline CreateGraphicsPipeline(in GraphicsPipelineDescription graphicsPipelineDescription);

    public IRenderIndexBuffer CreateIndexBuffer(uint length, string name);
}
