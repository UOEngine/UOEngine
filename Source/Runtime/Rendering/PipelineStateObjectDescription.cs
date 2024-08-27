using Silk.NET.Vulkan;

namespace UOEngine.Runtime.Rendering
{
    public readonly struct PipelineStateObjectDescription(Pipeline pipeline,
                                                          DescriptorSetLayout[] descriptorSetLayouts,
                                                          DescriptorSetLayoutBinding[] bindingDescriptions,
                                                          PipelineLayout pipelineLayout)
    {
        public readonly Pipeline                        GraphicsPipeline { get; } = pipeline;
        public readonly PipelineLayout                  Layout { get; } = pipelineLayout;
        public readonly DescriptorSetLayout[]           DescriptorSetLayouts {  get; } = descriptorSetLayouts;

        public readonly DescriptorSetLayoutBinding[]    BindingDescriptions { get; } = bindingDescriptions;

    }
}
