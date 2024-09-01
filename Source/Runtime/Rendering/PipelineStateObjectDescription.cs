using Silk.NET.Vulkan;

namespace UOEngine.Runtime.Rendering
{
    public class PipelineStateObjectDescription
    {
        public Pipeline                         GraphicsPipeline { get; private set; }
        public PipelineLayout                   Layout { get; private set; }
        public DescriptorSetLayout[]            DescriptorSetLayouts { get; private set; }

        public DescriptorSetLayoutBinding[]     BindingDescriptions { get; private set; }
        public PipelineStateObjectDescription(Pipeline pipeline,
                                              DescriptorSetLayout[] descriptorSetLayouts,
                                              DescriptorSetLayoutBinding[] bindingDescriptions,
                                              PipelineLayout pipelineLayout)
        {
            GraphicsPipeline = pipeline;
            Layout = pipelineLayout;
            DescriptorSetLayouts = descriptorSetLayouts;
            BindingDescriptions = bindingDescriptions;
        }

    }
}
