// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Vulkan;

[Service(UOEServiceLifetime.Singleton, typeof(IRenderResourceFactory))]
internal class VulkanResourceFactory: IRenderResourceFactory
{

    public RhiShaderResource NewShaderResource(in RhiShaderResourceCreateParameters createParameters = default)
    {
        throw new NotImplementedException();
    }

    public ShaderInstance NewShaderInstance(RhiShaderResource shaderResource)
    {
        throw new NotImplementedException();
    }

    public IRenderTexture CreateTexture(in RhiTextureDescription description)
    {
        throw new NotImplementedException();
    }

    public IRhiGraphicsPipeline CreateGraphicsPipeline(in RhiGraphicsPipelineDescription graphicsPipelineDescription)
    {
        throw new NotImplementedException();
    }

    public IRhiIndexBuffer CreateIndexBuffer(uint length, string name)
    {
        throw new NotImplementedException();
    }

    public IRhiVertexBuffer CreateVertexBuffer(in RhiVertexBufferDescription description)
    {
        throw new NotImplementedException();
    }
}
