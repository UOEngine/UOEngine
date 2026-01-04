// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.RHI;
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

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

    public VulkanGraphicsContext(VulkanDevice device)
    {
        _device = device;
    }

    public void BeginRecording(VulkanCommandBuffer commandBuffer)
    {
        _vertexBuffer = null;
        _indexBuffer = null;

        _commandBuffer = commandBuffer;
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

    internal unsafe void TransitionImageLayout(VkImage image, VkImageLayout oldLayout,VkImageLayout newLayout)
    {
        CommandBuffer.TransitionImageLayout(image, oldLayout, newLayout);
    }

    private void BindShaderParameters(bool forceRebind)
    {
        for (int i = 0; i < (int)ShaderProgramType.Count; i++)
        {
            ref var bindings = ref ShaderInstance.BindingData[i].Bindings;

            if (bindings is null)
            {
                continue;
            }

            if(bindings.Length == 0)
            {
                continue;
            }

            UOEDebug.NotImplemented();

            switch ((ShaderProgramType)i)
            {
                case ShaderProgramType.Vertex:
                    {
                        //BindParametersForVertexProgram(bindings, forceRebind);

                        break;
                    }
                case ShaderProgramType.Pixel:
                    {
                        //BindParametersForPixelProgram(bindings, forceRebind);

                        break;
                    }
                default:
                    throw new UnreachableException("No bind parameters implements.");
            }
        }
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

            if (_pipelineCache.TryGetValue(_currentPipelineDescription, out var graphicsPipeline) == false)
            {
                _graphicsPipeline = new VulkanGraphicsPipeline(_device, _currentPipelineDescription.Description, _currentPipelineDescription.AttachmentFormat);

                _pipelineCache.Add(_currentPipelineDescription, _graphicsPipeline);
            }

            VkViewport viewport = new()
            {
                x = 0.0f,
                y = 0.0f,
                width = _viewport.width,
                height = _viewport.height,
                minDepth = 0.0f,
                maxDepth = 1.0f
            };

            _device.Api.vkCmdSetViewport(commandBuffer, 0, viewport);

            // Update dynamic scissor state
            VkRect2D scissor = new(0, 0, (uint)viewport.width, (uint)viewport.height);

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
