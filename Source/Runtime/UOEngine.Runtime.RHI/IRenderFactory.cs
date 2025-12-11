using UOEngine.Runtime.RHI.Resources;

namespace UOEngine.Runtime.RHI;

public interface IRenderResourceFactory
{
    public RhiShaderResource NewShaderResource(in RhiShaderResourceCreateParameters createParameters = default);
    public ShaderInstance NewShaderInstance(RhiShaderResource shaderResource);

    public IRenderTexture CreateTexture(in RhiTextureDescription description);

    public IGraphicsPipeline CreateGraphicsPipeline(in RhiGraphicsPipelineDescription graphicsPipelineDescription);

    public IRhiIndexBuffer CreateIndexBuffer(uint length, string name);

    public IRhiVertexBuffer CreateVertexBuffer(in RhiVertexBufferDescription description);
}
