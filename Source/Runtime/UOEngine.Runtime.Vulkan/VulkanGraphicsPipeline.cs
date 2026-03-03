// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Text;

using Vortice.Vulkan;

using UOEngine.Runtime.RHI;
using UOEngine.Runtime.Core;

namespace UOEngine.Runtime.Vulkan;

internal class VulkanGraphicsPipeline: IDisposable
{
    public VkPipeline Handle { get; private set; }

    public readonly VkPipelineLayout PipelineLayout;
    private bool disposedValue;

    public VkDescriptorSetLayout DescriptorSetLayout { get; private set; }

    private readonly VulkanDevice _device;

    internal unsafe VulkanGraphicsPipeline(VulkanDevice device, in RhiGraphicsPipelineDescription pipelineDescription, VkFormat attachmentFormat)
    {
        _device = device;

        VulkanShaderResource shaderResource = (VulkanShaderResource)pipelineDescription.Shader.ShaderResource;

        VkUtf8ReadOnlyString vertexEntryPoint = Encoding.UTF8.GetBytes(shaderResource.VertexProgram.EntryPoint);

        Span<VkPipelineShaderStageCreateInfo> shaderStages = stackalloc VkPipelineShaderStageCreateInfo[2];

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

        Span<VkDescriptorSetLayoutBinding> descriptorSetLayoutBindings = stackalloc VkDescriptorSetLayoutBinding[numDescriptorSets];

        int descriptorIndex = 0;

        void AddDescriptorSetLayout(VkShaderStageFlags shaderStage, ShaderParameter[] shaderParameters, Span<VkDescriptorSetLayoutBinding> descriptorSetLayoutBindings)
        {
            foreach(var shaderParameter in shaderParameters)
            {
                if(shaderParameter.Space != 0)
                {
                    UOEDebug.NotImplemented("Only space/set 0 is currently supported.");
                }

                ref var descriptorSetLayoutBinding = ref descriptorSetLayoutBindings[descriptorIndex];

                descriptorSetLayoutBinding.stageFlags = shaderStage;
                descriptorSetLayoutBinding.binding = shaderParameter.SlotIndex;
                descriptorSetLayoutBinding.descriptorType = shaderParameter.InputType.ToVkDescriptorType();
                descriptorSetLayoutBinding.descriptorCount = 1;

                descriptorIndex++;
            }
        }

        AddDescriptorSetLayout(VkShaderStageFlags.Vertex, shaderResource.VertexProgram.InputBindings, descriptorSetLayoutBindings);
        AddDescriptorSetLayout(VkShaderStageFlags.Fragment, shaderResource.PixelProgram.InputBindings, descriptorSetLayoutBindings);

        VkDescriptorSetLayout descriptorSetLayout;

        fixed (VkDescriptorSetLayoutBinding* pDescriptorSetLayoutBinding = descriptorSetLayoutBindings)
        {
            VkDescriptorSetLayoutCreateInfo descriptorSetLayoutCreateInfo = new()
            {
                bindingCount = (uint)numDescriptorSets,
                pBindings = pDescriptorSetLayoutBinding
            };

            device.Api.vkCreateDescriptorSetLayout(device.Handle, descriptorSetLayoutCreateInfo, out descriptorSetLayout);
        }

        VkPipelineLayoutCreateInfo pipelineLayoutCreateInfo = new()
        {
           setLayoutCount = 1,
           pSetLayouts = &descriptorSetLayout
        };

        DescriptorSetLayout = descriptorSetLayout;

        device.Api.vkCreatePipelineLayout(device.Handle, pipelineLayoutCreateInfo, out PipelineLayout);

        VkPipelineInputAssemblyStateCreateInfo inputAssemblyState = new(pipelineDescription.PrimitiveType.ToVkPrimitiveTopology());

        VkPipelineViewportStateCreateInfo viewportState = new(1, 1);

        // Rasterization state
        VkPipelineRasterizationStateCreateInfo rasterizationState = new()
        {
            cullMode = pipelineDescription.Rasteriser.CullMode.ToVkCullModeFlags(),
            frontFace = VkFrontFace.Clockwise,// pipelineDescription.Rasteriser.FrontFace.ToVkFrontFace(),
            lineWidth = 1.0f
        };

        // Multi sampling state
        VkPipelineMultisampleStateCreateInfo multisampleState = VkPipelineMultisampleStateCreateInfo.Default;

        // DepthStencil
        VkPipelineDepthStencilStateCreateInfo depthStencilState = new()
        {
            depthTestEnable = false,
            depthWriteEnable = false
        };

        //pipelineDescription.BlendState.

        // BlendStates
        VkPipelineColorBlendAttachmentState blendAttachmentState = new()
        {
            blendEnable = pipelineDescription.BlendState.Enabled,
            srcColorBlendFactor = pipelineDescription.BlendState.SourceColourFactor.ToVkBlendFactor(),
            dstColorBlendFactor = pipelineDescription.BlendState.DestinationColourFactor.ToVkBlendFactor(),
            colorBlendOp = pipelineDescription.BlendState.ColourBlendOp.ToVkBlendOp(),
            srcAlphaBlendFactor = pipelineDescription.BlendState.SourceAlphaFactor.ToVkBlendFactor(),
            dstAlphaBlendFactor = pipelineDescription.BlendState.DestinationAlphaFactor.ToVkBlendFactor(),
            alphaBlendOp = pipelineDescription.BlendState.AlphaBlendOp.ToVkBlendOp(),
            colorWriteMask = VkColorComponentFlags.All
        };


        VkPipelineColorBlendStateCreateInfo colourBlendState = new()
        {
            pAttachments = &blendAttachmentState,
            attachmentCount = 1
        };

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
        int numAttributes = pipelineDescription.VertexLayout?.Attributes.Length ?? shaderResource.VertexProgram.StreamBindings.Length;

        Span<VkVertexInputAttributeDescription> vertexInputAttributes = stackalloc VkVertexInputAttributeDescription[numAttributes];

        if (pipelineDescription.VertexLayout != null)
        {
            vertexInputBinding = new(pipelineDescription.VertexLayout.Stride);

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
            uint offset = 0;

            for (var i = 0; i < numAttributes; i++)
            {
                ref var attribute = ref vertexInputAttributes[i];

                attribute.binding = 0; // Assume slot 0 for now.
                attribute.location = shaderResource.VertexProgram.StreamBindings[i].SemanticIndex;
                attribute.offset = offset;
                attribute.format = shaderResource.VertexProgram.StreamBindings[i].Format.ToVkFormat();

                offset += shaderResource.VertexProgram.StreamBindings[i].Format.ToSize();
            }
        }

        fixed (VkVertexInputAttributeDescription* pVertexInputAttributes = vertexInputAttributes)
        fixed (VkPipelineShaderStageCreateInfo* pStages = shaderStages)
        {
            VkPipelineVertexInputStateCreateInfo vertexInputState = new()
            {
                vertexBindingDescriptionCount = 1,
                pVertexBindingDescriptions = &vertexInputBinding,
                vertexAttributeDescriptionCount = (uint)numAttributes,
                pVertexAttributeDescriptions = pVertexInputAttributes
            };

            VkGraphicsPipelineCreateInfo graphicsPipelineCreateInfo = new()
            {
                pNext = &renderingInfo,
                stageCount = 2,
                pStages = pStages,
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

            device.Api.vkCreateGraphicsPipeline(device.Handle, graphicsPipelineCreateInfo, out var pipeline);
            Handle = pipeline;
        }
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
            _device.Api.vkDestroyPipeline(_device.Handle, Handle);

            Handle = VkPipeline.Null;

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
