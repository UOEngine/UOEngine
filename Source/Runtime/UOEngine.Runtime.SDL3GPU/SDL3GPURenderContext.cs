// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;
using System.Runtime.CompilerServices;

using static SDL3.SDL;

using UOEngine.Runtime.RHI;
using UOEngine.Runtime.SDL3GPU.Resources;

namespace UOEngine.Runtime.SDL3GPU;

internal class SDL3GPURenderContext: IRenderContext
{
    public IntPtr RecordedCommands { get; private set; }

    public ShaderInstance ShaderInstance
    {
        get => _shaderInstance ?? throw new InvalidOperationException("ShaderInstance is null");
        set 
        { 
            if(ReferenceEquals(_shaderInstance, value))
            {
                return;
            }

            _shaderInstance = value;

            SetDirtyState(DirtyState.ShaderParams);
        }
    }

    public IRhiGraphicsPipeline GraphicsPipline 
    {
        set
        {
            if (ReferenceEquals(_graphicsPipeline, value))
            {
                return;
            }

            _graphicsPipeline = (Sdl3GpuGraphicsPipeline)value;

            SetDirtyState(DirtyState.Pipeline);
        }
    }

    public IRhiIndexBuffer? IndexBuffer
    {
        set
        {
            if (ReferenceEquals(_indexBuffer, value))
            {
                return;
            }

            if(value == null)
            {
                _indexBuffer = null;
            }
            else
            {

                _indexBuffer = (Sdl3GpuIndexBuffer)value;
            }

            SetDirtyState(DirtyState.IndexBuffer);
        }
    }

    public IRhiVertexBuffer VertexBuffer
    {
        get => _vertexBuffer ?? throw new InvalidOperationException("VertexBuffer is not set");
        set
        {
            if (ReferenceEquals(_vertexBuffer, value))
            {
                return;
            }

            if(value == null)
            {
                _vertexBuffer = null;
            }
            else
            {
                _vertexBuffer = (Sdl3GpuVertexBuffer)value;
            }


            SetDirtyState(DirtyState.VertexBuffer);
        }
    }

    public ModelViewProjection MVP 
    { 
        get => _sceneView; 
        set
        {
            _sceneView = value;

            unsafe
            {
                fixed(ModelViewProjection* data = &_sceneView)
                {
                    SDL_PushGPUVertexUniformData(RecordedCommands, 0, (IntPtr)data, (uint)sizeof(ModelViewProjection));
                }
            }
        }
    }

    public RhiSampler Sampler
    {
        get => _sampler;
        set
        {
            if (_sampler == value)
            {
                return;
            }

            _sampler = value;
        }
    }

    private IntPtr _renderPass;
    private ShaderInstance? _shaderInstance;
    private Sdl3GpuGraphicsPipeline? _graphicsPipeline;
    private readonly Sdl3GpuDevice _device;

    private DirtyState _dirtyState = DirtyState.All;

    private RenderPassInfo? _activeRenderPass;

    private Sdl3GpuIndexBuffer? _indexBuffer;
    private Sdl3GpuVertexBuffer? _vertexBuffer;
    private ModelViewProjection _sceneView;

    private readonly Sdl3GpuGlobalSamplers _globalSamplers;

    private readonly Dictionary<RhiGraphicsPipelineDescription, Sdl3GpuGraphicsPipeline> _pipelineCache = [];

    private RhiGraphicsPipelineDescription _currentPipelineDescription;
    private Sdl3GpuGraphicsPipeline? _currentPipeline;

    private RhiSampler _sampler;

    [Flags]
    private enum DirtyState
    {
        None =              0,
        Pipeline =     1 << 0,
        VertexBuffer = 1 << 1,
        IndexBuffer =  1 << 2,
        ShaderParams = 1 << 3,
        All = Pipeline|VertexBuffer|IndexBuffer|ShaderParams
    }

    public SDL3GPURenderContext(Sdl3GpuDevice device, Sdl3GpuGlobalSamplers globalSamplers)
    {
        _device = device;
        _globalSamplers = globalSamplers;
    }

    public void BeginRecording()
    {
        RecordedCommands = SDL_AcquireGPUCommandBuffer(_device.Handle);

        _graphicsPipeline = null;
        _activeRenderPass = null;
        _indexBuffer = null;

        SetDirtyState(DirtyState.All);

        Debug.Assert(RecordedCommands != IntPtr.Zero);
    }

    public void EndRecording()
    {
        SDL_SubmitGPUCommandBuffer(RecordedCommands);
    }

    public void BeginRenderPass(in RenderPassInfo renderPassInfo)
    {
        Debug.Assert(_renderPass ==  IntPtr.Zero);

        _activeRenderPass = renderPassInfo;

        SDL_GPUColorTargetInfo colourTargetInfo = new()
        {
            texture = (renderPassInfo.RenderTarget.Texture as SDL3GPUTexture)!.Handle,
            mip_level = 0,
            layer_or_depth_plane = 0,
            // Note must always clear to 0 otherwise SDK layer complains.
            // Okay for now as usually do not need a clear colour other than "empty".
            clear_color = new()
            {
                r = 0.0f,
                g = 0.0f, 
                b = 0.0f,
                a = 0.0f
            },
            load_op = SDL_GPULoadOp.SDL_GPU_LOADOP_CLEAR,
            store_op = SDL_GPUStoreOp.SDL_GPU_STOREOP_STORE,
            resolve_texture = IntPtr.Zero,
            resolve_layer = 0,
            cycle = false,
            cycle_resolve_texture = false,
        };
        
        _renderPass = SDL_BeginGPURenderPass(RecordedCommands, [colourTargetInfo], 1, Unsafe.NullRef<SDL_GPUDepthStencilTargetInfo>());

        Debug.Assert(_renderPass != IntPtr.Zero);
    }

    public void EndRenderPass()
    {
        Debug.Assert(_renderPass != IntPtr.Zero);

        SDL_EndGPURenderPass(_renderPass);

        _renderPass = IntPtr.Zero;
        _activeRenderPass = null;
    }

    public void DrawIndexedPrimitives(uint numIndices, uint numInstances, uint firstIndex, uint vertexOffset, uint firstInstance)
    {
        if(numIndices == 24570)
        {
            ;
        }

       FlushIfNeeded();

       SDL_DrawGPUIndexedPrimitives(_renderPass, numIndices, numInstances, firstIndex, (int)vertexOffset, firstInstance);

    }

    public void SetGraphicsPipeline(in RhiGraphicsPipelineDescription graphicsPipelineDescription)
    {
        if (graphicsPipelineDescription == _currentPipelineDescription)
        {
            return;
        }

        SetDirtyState(DirtyState.Pipeline);
        SetDirtyState(DirtyState.ShaderParams);

        ShaderInstance = graphicsPipelineDescription.Shader;
        _currentPipelineDescription = graphicsPipelineDescription;
    }

    public void SetSampler(in RhiSampler sampler)
    {
        _sampler = sampler;
    }

    public void WaitForGpuIdle()
    {
        _device.WaitForGpuIdle();
    }

    private void BindShaderParameters(bool forceRebind)
    {
        for(int i = 0; i < (int)ShaderProgramType.Count; i++)
        {
            ref var bindings = ref ShaderInstance.BindingData[i].Bindings;

            if(bindings is null)
            {
                continue;
            }

            switch((ShaderProgramType)i)
            {
                case ShaderProgramType.Vertex:
                    {
                        BindParametersForVertexProgram(bindings, forceRebind);

                        break;
                    }
                case ShaderProgramType.Pixel:
                    {
                        BindParametersForPixelProgram(bindings, forceRebind);

                        break;
                    }
                default:
                    throw new UnreachableException("No bind parameters implements.");
            }
        }
    }

    private void BindParametersForVertexProgram(ShaderBindingDataEntry[] bindingEntries, bool forceRebind)
    {
        for(int i = 0; i < bindingEntries.Length; i++)
        {
            ref var entry = ref bindingEntries[i];

            if((entry.Dirty == false) && (forceRebind == false))
            {
                continue;
            }

            switch(entry.InputType)
            {
                case RhiShaderInputType.Buffer:
                case RhiShaderInputType.Constant:
                    {
                        unsafe
                        {
                            fixed(byte* ptr = entry.Data.Buffer)
                            {
                                SDL_PushGPUVertexUniformData(RecordedCommands, entry.BindingIndex, (IntPtr)ptr, (uint)entry.Data.Buffer.Length);
                            }
                        }

                        break;
                    }

                default:
                    throw new UnreachableException("BindParametersForVertexProgram: Unhandled input type");
            }

            entry.Dirty = false;
        }
    }

    private void BindParametersForPixelProgram(ShaderBindingDataEntry[] bindingEntries, bool forceRebind)
    {
        Span<SDL_GPUTextureSamplerBinding> samplerBindings = stackalloc SDL_GPUTextureSamplerBinding[bindingEntries.Length / 2];

        uint numBindings = 0;

        for(int i = 0;  i < bindingEntries.Length; i++)
        {
            ref var entry = ref bindingEntries[i];

            if ((entry.Dirty == false) && (forceRebind == false))
            {
                continue;
            }

            switch (entry.InputType)
            {
                case RhiShaderInputType.Texture:
                    {
                        samplerBindings[(int)entry.BindingIndex].texture = entry.Texture.Handle;

                        if(samplerBindings[(int)entry.BindingIndex].sampler == IntPtr.Zero)
                        {
                            samplerBindings[(int)entry.BindingIndex].sampler = _globalSamplers.GetSampler(_sampler).Handle;
                        }

                        numBindings++;

                        break;
                    }

                case RhiShaderInputType.Sampler:
                    {
                        samplerBindings[(int)entry.BindingIndex].sampler = _globalSamplers.GetSampler(_sampler).Handle;
                        break;
                    }
                default:
                    throw new UnreachableException("BindParametersForPixelProgram: Unhandled input type");
            }

            entry.Dirty = false;
        }

        SDL_BindGPUFragmentSamplers(_renderPass,  0, samplerBindings, numBindings);
    }

    private void FlushIfNeeded()
    {
        if (IsStateDirty(DirtyState.VertexBuffer))
        {
            _vertexBuffer!.Bind(_renderPass);

            ClearDirtyState(DirtyState.VertexBuffer);

            if(_currentPipelineDescription.VertexLayout != _vertexBuffer.VertexDefinition)
            {
                _currentPipelineDescription.VertexLayout = _vertexBuffer.VertexDefinition;

                SetDirtyState(DirtyState.Pipeline);
            }
        }

        if (IsStateDirty(DirtyState.Pipeline))
        {
            if (_pipelineCache.TryGetValue(_currentPipelineDescription, out _graphicsPipeline) == false)
            {
                _graphicsPipeline = new Sdl3GpuGraphicsPipeline(_device, _currentPipelineDescription);

                _pipelineCache.Add(_currentPipelineDescription, _graphicsPipeline);
            }

            SDL_BindGPUGraphicsPipeline(_renderPass, _graphicsPipeline.Handle);

            SetDirtyState(DirtyState.ShaderParams);
            ClearDirtyState(DirtyState.Pipeline);
        }

        if (IsStateDirty(DirtyState.IndexBuffer))
        {
            _indexBuffer!.Bind(_renderPass);

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
