// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.RHI;

public interface IRenderResourceFactory
{
    public RhiShaderResource NewShaderResource(in RhiShaderResourceCreateParameters createParameters = default);
    public ShaderInstance NewShaderInstance(RhiShaderResource shaderResource);

    public IRenderTexture CreateTexture(in RhiTextureDescription description);

    public IRhiGraphicsPipeline CreateGraphicsPipeline(in RhiGraphicsPipelineDescription graphicsPipelineDescription);

    public IRhiBuffer NewBuffer(in RhiBufferDescription bufferDescription);
}
