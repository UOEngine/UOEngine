using Silk.NET.Vulkan;

namespace UOEngine.Runtime.Rendering
{
    public readonly struct PipelineStateObjectDescription(Pipeline pso, PipelineLayout layout, 
                                                          DescriptorSetLayout[] descriptorSetLayouts, 
                                                          SetBindingDescription[] bindingDescriptions)
    {
        public readonly Pipeline                    PSO { get; } = pso;
        public readonly PipelineLayout              Layout { get; } = layout;
        public readonly DescriptorSetLayout[]       DescriptorSetLayouts {  get; } = descriptorSetLayouts;

        public readonly SetBindingDescription[]     BindingDescriptions { get; } = bindingDescriptions;

    }
}
