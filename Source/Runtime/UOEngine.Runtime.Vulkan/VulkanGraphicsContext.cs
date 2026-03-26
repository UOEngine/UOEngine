// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;

using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

using UOEngine.Runtime.Core;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Vulkan;

internal struct VulkanGraphicsContextInit
{
    internal required VulkanRenderer Renderer;
    internal required VulkanDevice Device;
    internal required int Id;
    internal required VulkanGlobalSamplers GlobalSamplers;
}

[DebuggerDisplay("VulkanGraphicsContext{_id} {_currentName}")]
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
            //if (ReferenceEquals(_indexBuffer, value))
            //{
            //    return;
            //}

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
            //if (ReferenceEquals(_vertexBuffer, value))
            //{
            //    return;
            //}

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

    public RhiSampler Sampler
    {
        get => _sampler;
        set => _sampler = Sampler;
    }

    internal VulkanCommandBuffer CommandBuffer => _commandBuffer ?? throw new InvalidOperationException("VulkanGraphicsContext: CommandBuffer not initialised");

    public bool IsInRenderPass { get; private set; } = false;

    //internal VulkanFence SubmitFence = null!;

    internal VkPipelineStageFlags[] WaitStages = [];
    internal VkSemaphore[] WaitForSemaphores = [];

    internal VkSemaphore[] SignalSemaphores = [];

    internal bool IsRecording { get; private set; } = false;

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

    private VulkanTexture? _defaultBackbufferRenderTarget;

    private RhiSampler _sampler;

    private string _currentName = "";

    private VulkanRenderer _renderer;

    private VulkanCommandBufferPool _pool;

    private readonly int _id;

    public VulkanGraphicsContext(in VulkanGraphicsContextInit init)
    {
        _device = init.Device;
        _globalSamplers = init.GlobalSamplers;
        _renderer = init.Renderer;

        _pool = _device.GraphicsQueue.AllocatePool();

        _descriptorPool = new VulkanDescriptorPool(_device);
        _id = init.Id;
        _uniformBufferObjectScratchAllocator = new VulkanScratchBlockAllocator(_device, $"UniformBufferScratchAllocator{init.Id}");
    }

    public void TransitionTextureUsage(IRenderTexture texture, RhiRenderTextureUsage usage) => CommandBuffer.EnsureState((VulkanTexture)texture, usage);

    public void BeginRecording()
    {
        UOEDebug.Assert(IsRecording == false);

        _vertexBuffer = null;
        _indexBuffer = null;

        _graphicsPipeline = null;

        _dirtyState = DirtyState.All;

        _commandBuffer!.BeginRecording();
        _uniformBufferObjectScratchAllocator.Reset();
        _descriptorPool.Reset();

        SignalSemaphores = [];
        WaitStages = [];
        WaitForSemaphores = [];

        IsRecording = true;
    }
    public void EndRecording()
    {
        if(IsInRenderPass)
        {
            EndRenderPass();
        }

        if(IsRecording)
        {
            _device.Api.vkEndCommandBuffer(CommandBuffer.Handle).CheckResult();
        }

        IsRecording = false;
    }

    public unsafe void BeginRenderPass(in RenderPassInfo renderPassInfo)
    {
        UOEDebug.Assert(IsInRenderPass == false, "Can not begin renderpass when already in one.");

        VulkanTexture texture = _defaultBackbufferRenderTarget!;

        if (renderPassInfo.RenderTarget != null)
        {
            texture = (VulkanTexture)renderPassInfo.RenderTarget.Texture;
        }

        CommandBuffer.EnsureState(texture, RhiRenderTextureUsage.ColourTarget);

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

    public void EndRenderPass()
    {
        UOEDebug.Assert(IsInRenderPass, "Can not end renderpass when not in one.");

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

    public void Flush()
    {
        EndRecording();

        _renderer.SubmitImmediate(this);
    }

    internal void Prepare(VulkanTexture defaultRenderTarget, string name)
    {
        _defaultBackbufferRenderTarget = defaultRenderTarget;
        _currentName = name;

        _commandBuffer = _pool.Create();

        CommandBuffer.Name = _currentName;

        _uniformBufferObjectScratchAllocator.Reset();

        //UOEDebug.Assert(SubmitFence.IsSignaled);

        //SubmitFence.Reset();
    }

    private unsafe void BindShaderParameters(bool forceRebind)
    {
        Span<VkWriteDescriptorSet> writeDescriptorSets = stackalloc VkWriteDescriptorSet[ShaderInstance.NumBindings];

        uint numDescriptorsToUpdate = 0;

        VkDescriptorSet descriptorSet = _descriptorPool.Allocate(GraphicsPipeline.DescriptorSetLayout);

        int maxNumBuffers = 4;
        int maxNumImages = 4;

        VkDescriptorBufferInfo* bufferInfos = stackalloc VkDescriptorBufferInfo[maxNumBuffers];
        VkDescriptorImageInfo* imageInfos = stackalloc VkDescriptorImageInfo[maxNumImages];

        VulkanSampler sampler = _globalSamplers.PointClamp;

        VkDescriptorImageInfo imageSamplerInfo;

        imageSamplerInfo.sampler = sampler.Handle;

        int numBuffers = 0;
        int numImages = 0;

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

                            ref var bufferInfo = ref bufferInfos[numBuffers];

                            bufferInfo = new VkDescriptorBufferInfo
                            {
                                range = memoryAllocation.Size,
                                buffer = memoryAllocation.Buffer,
                                offset = memoryAllocation.Offset
                            };

                            descriptorWrite.pBufferInfo = &bufferInfos[numBuffers];

                            numBuffers++;

                            UOEDebug.Assert(numBuffers <= maxNumBuffers);

                            //memoryAllocation.FlushMappedMemory(_device);

                            break;
                        }

                    case RhiShaderInputType.Sampler:
                        {
                            descriptorWrite.descriptorType = VkDescriptorType.Sampler;

                            descriptorWrite.pImageInfo = &imageSamplerInfo;

                            break;
                        }
                    case RhiShaderInputType.Texture:
                        {
                            ref var imageInfo = ref imageInfos[numImages];

                            descriptorWrite.descriptorType = VkDescriptorType.SampledImage;

                            UOEDebug.Assert(((VulkanTexture)entry.GetTexture()).ImageView.IsNotNull);

                            imageInfo.imageView = ((VulkanTexture)entry.GetTexture()).ImageView;
                            imageInfo.imageLayout = VkImageLayout.ShaderReadOnlyOptimal;

                            descriptorWrite.pImageInfo = &imageInfos[numImages];

                            numImages++;

                            UOEDebug.Assert(numImages <= maxNumImages);


                            break;
                        }

                    default:
                        throw new UnreachableException($"BindParametersForVertexProgram: Unhandled input type {entry.InputType}");
                }

                writeDescriptorSets[(int)numDescriptorsToUpdate++] = descriptorWrite;

            }
        }

        if(numDescriptorsToUpdate > 0)
        {
            fixed(VkWriteDescriptorSet* pWriteDescriporSets = writeDescriptorSets)
            {
                _device.Api.vkUpdateDescriptorSets(_device.Handle, numDescriptorsToUpdate, pWriteDescriporSets, 0, null);
            }
        }

        _device.Api.vkCmdBindDescriptorSets(CommandBuffer.Handle, VkPipelineBindPoint.Graphics, GraphicsPipeline.PipelineLayout, 0, descriptorSet);
    }

    private void FlushIfNeeded()
    {
        UOEDebug.Assert(IsInRenderPass, "Must be in renderpass");

        VkCommandBuffer commandBuffer = _commandBuffer!.Handle;

        if (IsStateDirty(DirtyState.VertexBuffer))
        {
            _device.Api.vkCmdBindVertexBuffer(commandBuffer, 0, _vertexBuffer!.Handle);

            ClearDirtyState(DirtyState.VertexBuffer);

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

        bool fullRebind = true;// IsStateDirty(DirtyState.ShaderParams);

        BindShaderParameters(fullRebind);

        ClearDirtyState(DirtyState.ShaderParams);
    }

    private void SetDirtyState(DirtyState flags) => _dirtyState |= flags;

    private bool IsStateDirty(DirtyState flags) => (_dirtyState & flags) != 0;

    private void ClearDirtyState(DirtyState flags) => _dirtyState &= ~flags;
}
