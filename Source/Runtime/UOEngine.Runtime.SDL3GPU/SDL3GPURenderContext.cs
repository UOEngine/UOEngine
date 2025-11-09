using System.Diagnostics;

using static SDL3.SDL;

using UOEngine.Runtime.RHI;
using UOEngine.Runtime.RHI.Resources;
using UOEngine.Runtime.SDL3GPU.Resources;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace UOEngine.Runtime.SDL3GPU;

internal class SDL3GPURenderContext: IRenderContext
{
    public IntPtr RecordedCommands { get; private set; }

    public ShaderInstance ShaderInstance
    {
        get => _shaderInstance;
        set 
        { 
            if(_shaderInstance == value)
            {
                return;
            }

            _shaderInstance = value;
            _shaderInstanceDirty = true;
        }
    }

    public IGraphicsPipeline GraphicsPipline 
    {
        get => _graphicsPipeline;
        set
        {
            if (_graphicsPipeline == value)
            {
                return;
            }

            _graphicsPipeline = (Sdl3GpuGraphicsPipeline)value;
            _pipelineDirty = true;
        }
    }

    public IRenderIndexBuffer IndexBuffer
    {
        get => _indexBuffer;
        set
        {
            if (_indexBuffer == value)
            {
                return;
            }

            _indexBuffer = (Sdl3GpuIndexBuffer)value;
            _indexBufferDirty = true;
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

    private IntPtr _renderPass;
    private ShaderInstance _shaderInstance;
    private Sdl3GpuGraphicsPipeline? _graphicsPipeline;
    private readonly Sdl3GpuDevice _device;

    private bool _pipelineDirty = true;
    private bool _indexBufferDirty = true;
    private bool _shaderInstanceDirty = true;

    private RenderPassInfo? _activeRenderPass;

    private Sdl3GpuIndexBuffer _indexBuffer;
    private ModelViewProjection _sceneView;

    private readonly Sdl3GpuGlobalSamplers _globalSamplers;

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
        _pipelineDirty = true;
        _indexBuffer = null;

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

    public void DrawIndexedPrimitives(uint numInstances)
    {
        if(_pipelineDirty)
        {
            SDL_BindGPUGraphicsPipeline(_renderPass, _graphicsPipeline.Handle);

            _pipelineDirty = false;
            _shaderInstanceDirty = true;
        }

        if(_indexBufferDirty)
        {
            _indexBuffer.Bind(_renderPass);

            _indexBufferDirty = false;
        }

        if(_shaderInstanceDirty)
        {
            BindShaderParameters();

            _shaderInstanceDirty = false;
        }
        
        SDL_DrawGPUIndexedPrimitives(_renderPass, (uint)_indexBuffer.Data.Length, numInstances, 0, 0, 0);
    }

    private void BindShaderParameters()
    {
        for(int i = 0; i < (int)ShaderProgramType.Count; i++)
        {
            ref var bindings = ref _shaderInstance.BindingData[i].Bindings;

            if(bindings is null)
            {
                continue;
            }

            switch((ShaderProgramType)i)
            {
                case ShaderProgramType.Vertex:
                    {
                        BindParametersForVertexProgram(bindings);

                        break;
                    }
                case ShaderProgramType.Pixel:
                    {
                        BindParametersForPixelProgram(bindings);

                        break;
                    }
                default:
                    throw new UnreachableException("No bind parameters implements.");
            }
        }
    }

    private void BindParametersForVertexProgram(ShaderBindingDataEntry[] bindingEntries)
    {
        for(int i = 0; i < bindingEntries.Length; i++)
        {
            ref var entry = ref bindingEntries[i];

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
        }
    }

    private void BindParametersForPixelProgram(ShaderBindingDataEntry[] bindingEntries)
    {
        Span<SDL_GPUTextureSamplerBinding> samplerBindings = stackalloc SDL_GPUTextureSamplerBinding[1];

        var samplerBinding = new SDL_GPUTextureSamplerBinding();

        int i = 0;
        int numTextures = 0;

        foreach (var entry in bindingEntries)
        {
            switch(entry.InputType)
            {
                case RhiShaderInputType.Texture:
                    {
                        samplerBinding.texture = entry.Texture.Handle;
                        
                        break;
                    }

                case RhiShaderInputType.Sampler:
                    {
                        samplerBinding.sampler = _globalSamplers.GetSampler(entry.Sampler).Handle;
                        break;
                    }
                default:
                    throw new UnreachableException("BindParametersForPixelProgram: Unhandled input type");
            }

            i++;

            if(i % 2 == 0)
            {
                samplerBindings[numTextures++] = samplerBinding;
            }
        }

        SDL_BindGPUFragmentSamplers(_renderPass,  0, samplerBindings, (uint)samplerBindings.Length);
    }

}
