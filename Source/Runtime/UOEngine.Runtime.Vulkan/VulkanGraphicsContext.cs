// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;

using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

using UOEngine.Runtime.Core;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Vulkan;

internal class VulkanGraphicsContext : IRenderContext
{
    public ShaderInstance ShaderInstance
    {
        get => _shaderInstance ?? throw new InvalidOperationException("ShaderInstance is null");
        set
        {
            if (ReferenceEquals(_shaderInstance, value))
            {
                return;
            }

            _shaderInstance = value;

            SetDirtyState(DirtyState.ShaderParams);
        }
    }

    public IRhiBuffer IndexBuffer
    {
        set
        {
            if (ReferenceEquals(_indexBuffer, value))
            {
                return;
            }

            if (value == null)
            {
                _indexBuffer = null;
            }
            else
            {

                _indexBuffer = (VulkanBuffer)value;
            }

            SetDirtyState(DirtyState.IndexBuffer);
        }
    }

    public IRhiBuffer VertexBuffer
    {
        get => _vertexBuffer ?? throw new InvalidOperationException("VertexBuffer is not set");
        set
        {
            if (ReferenceEquals(_vertexBuffer, value))
            {
                return;
            }

            if (value == null)
            {
                _vertexBuffer = null;
            }
            else
            {
                _vertexBuffer = (VulkanBuffer)value;
            }

            SetDirtyState(DirtyState.VertexBuffer);
        }
    }

    public RhiSampler Sampler { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    internal VulkanCommandBuffer CommandBuffer => _commandBuffer ?? throw new InvalidOperationException("VulkanGraphicsContext: CommandBuffer not initialised");

    public bool IsInRenderPass { get; private set; }

    private readonly VulkanDevice _device;

    private VulkanCommandBuffer? _commandBuffer;

    private VulkanBuffer? _indexBuffer;
    private VulkanBuffer? _vertexBuffer;

    private ShaderInstance? _shaderInstance;

    [Flags]
    private enum DirtyState
    {
        None = 0,
        Pipeline = 1 << 0,
        VertexBuffer = 1 << 1,
        IndexBuffer = 1 << 2,
        ShaderParams = 1 << 3,
        All = Pipeline | VertexBuffer | IndexBuffer | ShaderParams
    }

    private DirtyState _dirtyState = DirtyState.All;

    private VulkanGraphicsPipelineDescription _currentPipelineDescription;
    private VkPipeline? _currentPipeline;

    private readonly Dictionary<VulkanGraphicsPipelineDescription, VulkanGraphicsPipeline> _pipelineCache = [];

    private VulkanGraphicsPipeline? _graphicsPipeline;

    private VulkanGraphicsPipeline GraphicsPipeline => _graphicsPipeline ?? throw new InvalidOperationException("VulkanGraphicsContext: GraphicsPipeline is not initialised.");

    private VkViewport _viewport = new(0.0f, 0.0f);
    private VkRect2D _scissor = new(0, 0, 0, 0);

    private VkRenderingInfo _renderingInfo = new();

    private VulkanTexture _renderTarget = null!;

    private readonly Dictionary<VkDescriptorSetLayout, VkDescriptorPool> _descriptorSetPools = [];

    private VulkanScratchBlockAllocator _uniformBufferObjectScratchAllocator = null!;
    private VulkanDescriptorPool _descriptorPool = null!;
    private readonly VulkanGlobalSamplers _globalSamplers;

    public VulkanGraphicsContext(VulkanDevice device, VulkanGlobalSamplers globalSamplers)
    {
        _device = device;
        _globalSamplers = globalSamplers;
    }

    public void BeginRecording(VulkanCommandBuffer commandBuffer, VulkanScratchBlockAllocator uniformBufferObjectScratchAllocator, VulkanDescriptorPool descriptorPool)
    {
        _vertexBuffer = null;
        _indexBuffer = null;

        _graphicsPipeline = null;

        _dirtyState = DirtyState.All;

        _commandBuffer = commandBuffer;
        _uniformBufferObjectScratchAllocator = uniformBufferObjectScratchAllocator;
        _descriptorPool = descriptorPool;
    }

    public unsafe void BeginRenderPass(in RenderPassInfo renderPassInfo)
    {
        VulkanTexture texture = (VulkanTexture)renderPassInfo.RenderTarget.Texture;

        _renderTarget = texture;

        VkRenderingAttachmentInfo colorAttachment = new()
        {
            imageView = texture.ImageView,
            imageLayout = VkImageLayout.ColorAttachmentOptimal,
            resolveMode = VkResolveModeFlags.None,
            loadOp = VkAttachmentLoadOp.Clear,
            storeOp = VkAttachmentStoreOp.Store,
            clearValue = new(0.0f, 0.0f, 0.0f)  // Red to debug
        };

        VkRenderingInfo renderingInfo = new()
        {
            renderArea = new VkRect2D(VkOffset2D.Zero, new(texture.Width, texture.Height)),
            layerCount = 1u,
            colorAttachmentCount = 1,
            pColorAttachments = &colorAttachment,
        };

        _device.Api.vkCmdBeginRendering(CommandBuffer.Handle, &renderingInfo);

        _viewport.width = texture.Width;
        _viewport.height = texture.Height;
        _scissor.extent.width = texture.Width;
        _scissor.extent.height = texture.Height;

        IsInRenderPass = true;
    }

    public void DrawIndexedPrimitives(uint numIndices, uint numInstances, uint firstIndex, uint vertexOffset, uint firstInstance)
    {
        VkCommandBuffer commandBuffer = _commandBuffer!.Handle;

        FlushIfNeeded();

        _device.Api.vkCmdDrawIndexed(commandBuffer, numIndices, numInstances, firstIndex, (int)vertexOffset, firstInstance);
    }

    public void EndRecording()
    {
        UOEDebug.Assert(IsInRenderPass == false);

        _device.Api.vkEndCommandBuffer(CommandBuffer.Handle).CheckResult();
    }

    public void EndRenderPass()
    {
        _device.Api.vkCmdEndRendering(CommandBuffer.Handle);

        IsInRenderPass = false;
    }

    public void SetGraphicsPipeline(in RhiGraphicsPipelineDescription graphicsPipelineDescription)
    {
        if (graphicsPipelineDescription == _currentPipelineDescription.Description)
        {
            return;
        }

        SetDirtyState(DirtyState.Pipeline);
        SetDirtyState(DirtyState.ShaderParams);

        ShaderInstance = graphicsPipelineDescription.Shader;
        _currentPipelineDescription.Description = graphicsPipelineDescription;
    }

    public void WaitForGpuIdle()
    {
        _device.WaitForGpuIdle();
    }

    internal unsafe void TransitionImageLayout(VkImage image, VkImageLayout oldLayout, VkImageLayout newLayout)
    {
        CommandBuffer.TransitionImageLayout(image, oldLayout, newLayout);
    }

    private unsafe void BindShaderParameters(bool forceRebind)
    {
        VkWriteDescriptorSet* writeDescriptorSets = stackalloc VkWriteDescriptorSet[ShaderInstance.NumBindings];

        uint numDescriptorsToUpdate = 0;

        VkDescriptorSet descriptorSet = _descriptorPool.Allocate(GraphicsPipeline.DescriptorSetLayout);

        VkDescriptorBufferInfo bufferInfo = new()
        {

        };

        for (int i = 0; i < (int)ShaderProgramType.Count; i++)
        {
            ref var bindings = ref ShaderInstance.BindingData[i].Bindings;

            if (bindings is null)
            {
                continue;
            }

            if (bindings.Length == 0)
            {
                continue;
            }

            foreach (var entry in bindings)
            {
                if ((entry.Dirty == false) && (forceRebind == false))
                {
                    continue;
                }

                VkWriteDescriptorSet descriptorWrite = new()
                {
                    dstBinding = entry.BindingIndex,
                    descriptorCount = 1,
                    dstSet = descriptorSet
                };

                switch (entry.InputType)
                {
                    case RhiShaderInputType.Buffer:
                    case RhiShaderInputType.Constant:
                        {
                            Span<byte> mappedMem = _uniformBufferObjectScratchAllocator.Allocate((uint)entry.Data.Buffer.Length, out var memoryAllocation);

                            entry.Data.Buffer.CopyTo(mappedMem);

                            descriptorWrite.descriptorType = VkDescriptorType.UniformBuffer;

                            bufferInfo.range = memoryAllocation.Size;
                            bufferInfo.buffer = memoryAllocation.Buffer;
                            bufferInfo.offset = memoryAllocation.Offset;

                            descriptorWrite.pBufferInfo = &bufferInfo;

                            memoryAllocation.FlushMappedMemory(memoryAllocation.Offset, memoryAllocation.Size);


                            break;
                        }

                    case RhiShaderInputType.Sampler:
                        {
                            descriptorWrite.descriptorType = VkDescriptorType.Sampler;

                            VulkanSampler sampler = _globalSamplers.PointClamp;

                            VkDescriptorImageInfo imageInfo;

                            imageInfo.sampler = sampler.Handle;

                            descriptorWrite.pImageInfo = &imageInfo;

                            break;
                        }
                    case RhiShaderInputType.Texture:
                        {
                            VkDescriptorImageInfo imageInfo;

                            descriptorWrite.descriptorType = VkDescriptorType.SampledImage;
                            
                            imageInfo.imageView = ((VulkanTexture)entry.GetTexture()).ImageView;
                            imageInfo.imageLayout = VkImageLayout.ShaderReadOnlyOptimal;

                            descriptorWrite.pImageInfo = &imageInfo;

                            break;
                        }

                    default:
                        throw new UnreachableException($"BindParametersForVertexProgram: Unhandled input type {entry.InputType}");
                }

                writeDescriptorSets[numDescriptorsToUpdate++] = descriptorWrite;

            }
        }

        if(numDescriptorsToUpdate > 0)
        {
            _device.Api.vkUpdateDescriptorSets(_device.Handle, numDescriptorsToUpdate, writeDescriptorSets, 0, null);
        }

        _device.Api.vkCmdBindDescriptorSets(CommandBuffer.Handle, VkPipelineBindPoint.Graphics, GraphicsPipeline.PipelineLayout, 0, descriptorSet);
    }

    private void FlushIfNeeded()
    {
        VkCommandBuffer commandBuffer = _commandBuffer!.Handle;

        if (IsStateDirty(DirtyState.VertexBuffer))
        {
            _device.Api.vkCmdBindVertexBuffer(commandBuffer, 0, _vertexBuffer!.Handle);

            ClearDirtyState(DirtyState.VertexBuffer);

            //if (_currentPipelineDescription.VertexLayout != _vertexBuffer.VertexDefinition)
            //{
            //    _currentPipelineDescription.VertexLayout = _vertexBuffer.VertexDefinition;

            //    SetDirtyState(DirtyState.Pipeline);
            //}
        }

        if (IsStateDirty(DirtyState.Pipeline))
        {
            _currentPipelineDescription.AttachmentFormat = _renderTarget.Description.Format;

            if (_pipelineCache.TryGetValue(_currentPipelineDescription, out _graphicsPipeline) == false)
            {
                _graphicsPipeline = new VulkanGraphicsPipeline(_device, _currentPipelineDescription.Description, _currentPipelineDescription.AttachmentFormat);

                _pipelineCache.Add(_currentPipelineDescription, _graphicsPipeline);
            }

            VkViewport viewport = new()
            {
                x = 0.0f,
                y = _viewport.height,
                width = _viewport.width,
                height = -_viewport.height,
                minDepth = 0.0f,
                maxDepth = 1.0f
            };

            _device.Api.vkCmdSetViewport(commandBuffer, 0, viewport);

            // Update dynamic scissor state
            VkRect2D scissor = new(0, 0, (uint)viewport.width, (uint)_viewport.height);

            _device.Api.vkCmdSetScissor(commandBuffer, 0, scissor);

            _device.Api.vkCmdBindPipeline(commandBuffer, VkPipelineBindPoint.Graphics, GraphicsPipeline.Handle);

            SetDirtyState(DirtyState.ShaderParams);
            ClearDirtyState(DirtyState.Pipeline);
        }

        if (IsStateDirty(DirtyState.IndexBuffer))
        {
            _device.Api.vkCmdBindIndexBuffer(commandBuffer, _indexBuffer!.Handle, 0, VkIndexType.Uint16);

            ClearDirtyState(DirtyState.IndexBuffer);
        }

        bool fullRebind = IsStateDirty(DirtyState.ShaderParams);


        BindShaderParameters(fullRebind);

        ClearDirtyState(DirtyState.ShaderParams);
    }

    private void SetDirtyState(DirtyState flags) => _dirtyState |= flags;

    private bool IsStateDirty(DirtyState flags) => (_dirtyState & flags) != 0;

    private void ClearDirtyState(DirtyState flags) => _dirtyState &= ~flags;
}
