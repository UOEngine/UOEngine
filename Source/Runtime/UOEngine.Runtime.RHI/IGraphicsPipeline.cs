using UOEngine.Runtime.RHI.Resources;

namespace UOEngine.Runtime.RHI;

public struct GraphicsPipelineDescription
{
    public RhiShaderResource ShaderResource;
    public string Name;
}

public interface IGraphicsPipeline
{
}
