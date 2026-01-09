// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Text;

using Vortice.Vulkan;

using UOEngine.Runtime.RHI;
using UOEngine.Runtime.Core;

namespace UOEngine.Runtime.Vulkan;

internal class VulkanGraphicsPipeline: IDisposable
{
    public readonly VkPipeline Handle;

    public readonly VkPipelineLayout PipelineLayout;
    private bool disposedValue;

    public VkDescriptorSetLayout DescriptorSetLayout { get; private set; }

    private readonly VulkanDevice _device;

    internal unsafe VulkanGraphicsPipeline(VulkanDevice device, in RhiGraphicsPipelineDescription pipelineDescription, VkFormat attachmentFormat)
    {
        _device = device;

        VulkanShaderResource shaderResource = (VulkanShaderResource)pipelineDescription.Shader.ShaderResource;

        VkUtf8ReadOnlyString vertexEntryPoint = Encoding.UTF8.GetBytes(shaderResource.VertexProgram.EntryPoint);

        VkPipelineShaderStageCreateInfo* shaderStages = stackalloc VkPipelineShaderStageCreateInfo[2];

        shaderStages[0] = new()
        {
            stage = VkShaderStageFlags.Vertex,
            module = shaderResource.VertexProgram.Handle,
            pName = vertexEntryPoint
        };

        VkUtf8ReadOnlyString pixelEntryPoint = Encoding.UTF8.GetBytes(shaderResource.PixelProgram.EntryPoint);

        shaderStages[1] = new()
        {
            stage = VkShaderStageFlags.Fragment,
            module = shaderResource.PixelProgram.Handle,
            pName = pixelEntryPoint
        };

        int numDescriptorSets = shaderResource.VertexProgram.InputBindings.Length + shaderResource.PixelProgram.InputBindings.Length;

        //VkDescriptorSetLayout* descriptorSetLayout = stackalloc VkDescriptorSetLayout[numDescriptorSets];

        //descriptorSetLayout[0] = shaderResource.VertexProgram.DescriptorSetLayout;
        //descriptorSetLayout[1] = shaderResource.PixelProgram.DescriptorSetLayout;

        VkDescriptorSetLayoutBinding* descriptorSetLayoutBindings = stackalloc VkDescriptorSetLayoutBinding[numDescriptorSets];

        int descriptorIndex = 0;

        void AddDescriptorSetLayout(VkShaderStageFlags shaderStage, ShaderParameter[] shaderParameters)
        {
            foreach(var shaderParameter in shaderParameters)
            {
                ref var descriptorSetLayoutBinding = ref descriptorSetLayoutBindings[descriptorIndex];

                descriptorSetLayoutBinding.stageFlags = shaderStage;
                descriptorSetLayoutBinding.binding = shaderParameter.SlotIndex;
                descriptorSetLayoutBinding.descriptorType = shaderParameter.InputType.ToVkDescriptorType();
                descriptorSetLayoutBinding.descriptorCount = 1;

                descriptorIndex++;
            }
        }

        AddDescriptorSetLayout(VkShaderStageFlags.Vertex, shaderResource.VertexProgram.InputBindings);
        AddDescriptorSetLayout(VkShaderStageFlags.Fragment, shaderResource.PixelProgram.InputBindings);

        VkDescriptorSetLayoutCreateInfo descriptorSetLayoutCreateInfo = new()
        {
            bindingCount = (uint)numDescriptorSets,
            pBindings = descriptorSetLayoutBindings
        };

        VkDescriptorSetLayout descriptorSetLayout;

        device.Api.vkCreateDescriptorSetLayout(device.Handle, descriptorSetLayoutCreateInfo, out descriptorSetLayout);

        VkPipelineLayoutCreateInfo pipelineLayoutCreateInfo = new()
        {
           setLayoutCount = (uint)numDescriptorSets,
           pSetLayouts = &descriptorSetLayout
        };

        DescriptorSetLayout = descriptorSetLayout;

        device.Api.vkCreatePipelineLayout(device.Handle, pipelineLayoutCreateInfo, out PipelineLayout);

        VkPipelineInputAssemblyStateCreateInfo inputAssemblyState = new(pipelineDescription.PrimitiveType.ToVkPrimitiveTopology());

        VkPipelineViewportStateCreateInfo viewportState = new(1, 1);

        // Rasterization state
        VkPipelineRasterizationStateCreateInfo rasterizationState = VkPipelineRasterizationStateCreateInfo.CullClockwise;

        // Multi sampling state
        VkPipelineMultisampleStateCreateInfo multisampleState = VkPipelineMultisampleStateCreateInfo.Default;

        // DepthStencil
        VkPipelineDepthStencilStateCreateInfo depthStencilState = VkPipelineDepthStencilStateCreateInfo.Default;

        // BlendStates
        VkPipelineColorBlendAttachmentState blendAttachmentState = default;
        blendAttachmentState.colorWriteMask = VkColorComponentFlags.All;
        blendAttachmentState.blendEnable = false;

        VkPipelineColorBlendStateCreateInfo colourBlendState = new(blendAttachmentState);

        VkDynamicState* dynamicStateEnables = stackalloc VkDynamicState[2];
        dynamicStateEnables[0] = VkDynamicState.Viewport;
        dynamicStateEnables[1] = VkDynamicState.Scissor;

        VkPipelineDynamicStateCreateInfo dynamicState = new()
        {
            dynamicStateCount = 2,
            pDynamicStates = dynamicStateEnables
        };

        VkPipelineRenderingCreateInfo renderingInfo = new()
        {
            colorAttachmentCount = 1u,
            pColorAttachmentFormats = &attachmentFormat
        };

        VkVertexInputBindingDescription vertexInputBinding;
        VkVertexInputAttributeDescription* vertexInputAttributes = stackalloc VkVertexInputAttributeDescription[16];

        if (pipelineDescription.VertexLayout != null)
        {
            vertexInputBinding = new(pipelineDescription.VertexLayout.Stride);

            int numAttributes = pipelineDescription.VertexLayout.Attributes.Length;

            uint location = 0;

            for (var i = 0; i < numAttributes; i++)
            {
                ref var attribute = ref vertexInputAttributes[i];
                ref var incomingAttribute = ref pipelineDescription.VertexLayout.Attributes[i];


                attribute.binding = 0; // Assume slot 0 for now.
                attribute.location = location++;
                attribute.offset = incomingAttribute.Offset;
                attribute.format = incomingAttribute.Format.ToVkFormat();

                //offset += MapToSize(incomingAttribute.Format);
            }
        }
        else
        {
            UOEDebug.NotImplemented();
            //for (var i = 0; i < vertexAttributes.Length; i++)
            //{
            //    ref var attribute = ref vertexAttributes[i];

            //    attribute.buffer_slot = 0; // Assume slot 0 for now.
            //    attribute.location = VertexProgram.StreamBindings[i].SemanticIndex;
            //    attribute.offset = offset;
            //    attribute.format = MapToSdl3GpuVertexElementFormat(VertexProgram.StreamBindings[i].Format);

            //    offset += MapToSize(VertexProgram.StreamBindings[i].Format);
            //}
        }

        VkPipelineVertexInputStateCreateInfo vertexInputState = new()
        {
            vertexBindingDescriptionCount = 1,
            pVertexBindingDescriptions = &vertexInputBinding,
            vertexAttributeDescriptionCount = 2,
            pVertexAttributeDescriptions = vertexInputAttributes
        };

        VkGraphicsPipelineCreateInfo graphicsPipelineCreateInfo = new()
        {
            pNext = &renderingInfo,
            stageCount = 2,
            pStages = shaderStages,
            pVertexInputState = &vertexInputState,
            pInputAssemblyState = &inputAssemblyState,
            pTessellationState = null,
            pViewportState = &viewportState,
            pRasterizationState = &rasterizationState,
            pMultisampleState = &multisampleState,
            pDepthStencilState = &depthStencilState,
            pColorBlendState = &colourBlendState,
            pDynamicState = &dynamicState,
            layout = PipelineLayout,
        };

        device.Api.vkCreateGraphicsPipeline(device.Handle, graphicsPipelineCreateInfo, out Handle);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            _device.Api.vkDestroyDescriptorSetLayout(_device.Handle, DescriptorSetLayout);

            DescriptorSetLayout = VkDescriptorSetLayout.Null;
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~VulkanGraphicsPipeline()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
