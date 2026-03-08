// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Vulkan;

[Service(UOEServiceLifetime.Singleton, typeof(IRenderResourceFactory))]
internal class VulkanResourceFactory: IRenderResourceFactory
{

    private VulkanDevice? _device;

    private VulkanDevice Device => _device ?? throw new InvalidOperationException("VulkanResourceFactory: VulkanDevice is not set.");

    public RhiShaderResource NewShaderResource(in RhiShaderResourceCreateParameters createParameters = default) => new VulkanShaderResource(Device, createParameters);

    public ShaderInstance NewShaderInstance(RhiShaderResource shaderResource) => new ShaderInstance(shaderResource);

    public IRenderTexture CreateTexture(in RhiTextureDescription description) =>  new VulkanTexture(_device!, description);

    public IRhiGraphicsPipeline CreateGraphicsPipeline(in RhiGraphicsPipelineDescription graphicsPipelineDescription)
    {
        throw new NotImplementedException();
    }

    public IRhiBuffer NewBuffer(in RhiBufferDescription bufferDescription) => new VulkanBuffer(Device, bufferDescription);

    internal void SetDevice(VulkanDevice device) => _device = device;
}
