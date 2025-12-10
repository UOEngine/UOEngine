using System.Diagnostics;

using static SDL3.SDL;

using UOEngine.Runtime.SDL3GPU.Resources;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.SDL3GPU;

internal class Sdl3GpuGraphicsPipeline : Sdl3GpuResource, IGraphicsPipeline
{
    public readonly SDL3GPUShaderProgram VertexProgram;
    public readonly SDL3GPUShaderProgram PixelProgram;

    public readonly RhiGraphicsPipelineDescription PipelineDescription;
    public readonly SDL_GPUGraphicsPipelineCreateInfo PipelineCreateInfo;

    public Sdl3GpuGraphicsPipeline(Sdl3GpuDevice device, in RhiGraphicsPipelineDescription graphicsPipelineDescription)
        : base(device)
    {
        PipelineDescription = graphicsPipelineDescription;

        Sdl3GpuShaderResource shaderResource = (Sdl3GpuShaderResource)graphicsPipelineDescription.Shader.ShaderResource;

        VertexProgram = shaderResource!.VertexProgram;
        PixelProgram = shaderResource.PixelProgram;

        SDL_GPUColorTargetDescription colourTargetDesc = new SDL_GPUColorTargetDescription
        {
            format = SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_B8G8R8A8_UNORM,
            blend_state = new SDL_GPUColorTargetBlendState
            {
                enable_blend = graphicsPipelineDescription.BlendState.Enabled,
                src_color_blendfactor = MapBlendFactor(graphicsPipelineDescription.BlendState.SourceColourFactor),
                src_alpha_blendfactor = MapBlendFactor(graphicsPipelineDescription.BlendState.SourceAlphaFactor),
                color_blend_op = MapBlendOp(graphicsPipelineDescription.BlendState.ColourBlendOp),
                alpha_blend_op = MapBlendOp(graphicsPipelineDescription.BlendState.AlphaBlendOp),
                dst_color_blendfactor = MapBlendFactor(graphicsPipelineDescription.BlendState.DestinationColourFactor),
                dst_alpha_blendfactor = MapBlendFactor(graphicsPipelineDescription.BlendState.DestinationAlphaFactor),
                color_write_mask = MapColourWriteMask(graphicsPipelineDescription.BlendState.WriteMask),
                enable_color_write_mask = true
            }
        };

        int numAttributes = graphicsPipelineDescription.VertexLayout?.Attributes.Length ?? VertexProgram.StreamBindings.Length;

        Span<SDL_GPUVertexAttribute> vertexAttributes = stackalloc SDL_GPUVertexAttribute[numAttributes];
        Span<SDL_GPUVertexBufferDescription> vertexBufferDescription = [];

        uint offset = 0;

        if (graphicsPipelineDescription.VertexLayout != null)
        {
            for (var i = 0; i < vertexAttributes.Length; i++)
            {
                ref var attribute = ref vertexAttributes[i];
                ref var incomingAttribute = ref graphicsPipelineDescription.VertexLayout.Attributes[i];

                attribute.buffer_slot = 0; // Assume slot 0 for now.
                attribute.location = VertexProgram.StreamBindings[i].SemanticIndex;
                attribute.offset = offset;
                attribute.format = MapToSdl3GpuVertexElementFormat(incomingAttribute.Format);

                offset += MapToSize(incomingAttribute.Format);
            }
        }
        else
        {
            for (var i = 0; i < vertexAttributes.Length; i++)
            {
                ref var attribute = ref vertexAttributes[i];

                attribute.buffer_slot = 0; // Assume slot 0 for now.
                attribute.location = VertexProgram.StreamBindings[i].SemanticIndex;
                attribute.offset = offset;
                attribute.format = MapToSdl3GpuVertexElementFormat(VertexProgram.StreamBindings[i].Format);

                offset += MapToSize(VertexProgram.StreamBindings[i].Format);
            }
        }

        if (vertexAttributes.Length > 0)
        {
            // Todo: Assume no instances for now.
            // We draw instanced with no vertex buffer as usually not required for quads.
            vertexBufferDescription = new SDL_GPUVertexBufferDescription[1];

            vertexBufferDescription[0].pitch = offset;
            vertexBufferDescription[0].instance_step_rate = 0;
        }

        var rasteriserState = new SDL_GPURasterizerState
        {
            cull_mode = graphicsPipelineDescription.Rasteriser.CullMode switch
            {
                RhiCullMode.Disable => SDL_GPUCullMode.SDL_GPU_CULLMODE_NONE,
                RhiCullMode.Back => SDL_GPUCullMode.SDL_GPU_CULLMODE_BACK,
                RhiCullMode.Front => SDL_GPUCullMode.SDL_GPU_CULLMODE_FRONT,
                _ => throw new NotImplementedException(),
            },
            fill_mode = graphicsPipelineDescription.Rasteriser.FillMode switch
            {
                RhiFillMode.Solid => SDL_GPUFillMode.SDL_GPU_FILLMODE_FILL,
                RhiFillMode.Wireframe => SDL_GPUFillMode.SDL_GPU_FILLMODE_LINE,
                _ => throw new NotImplementedException(),
            },
            front_face = SDL_GPUFrontFace.SDL_GPU_FRONTFACE_CLOCKWISE,
            //graphicsPipelineDescription.Rasteriser.FrontFace switch
            //{
            //    RhiFrontFace.Clockwise => SDL_GPUFrontFace.SDL_GPU_FRONTFACE_CLOCKWISE,
            //    RhiFrontFace.CounterClockwise => SDL_GPUFrontFace.SDL_GPU_FRONTFACE_COUNTER_CLOCKWISE,
            //    _ => throw new NotImplementedException(),
            //}
        };

        rasteriserState.cull_mode = SDL_GPUCullMode.SDL_GPU_CULLMODE_NONE;

        unsafe
        {
            fixed (SDL_GPUVertexAttribute* vertexAttributesPtr = vertexAttributes)
            fixed (SDL_GPUVertexBufferDescription* vertexBufferDescriptionPtr = vertexBufferDescription)
            {
                var createInfo = new SDL_GPUGraphicsPipelineCreateInfo
                {
                    vertex_shader = VertexProgram.Handle,
                    vertex_input_state = new SDL_GPUVertexInputState
                    {
                        vertex_attributes = vertexAttributesPtr,
                        num_vertex_attributes = (uint)vertexAttributes.Length,
                        vertex_buffer_descriptions = vertexBufferDescriptionPtr,
                        num_vertex_buffers = (uint)vertexBufferDescription.Length

                    },
                    fragment_shader = PixelProgram.Handle,
                    primitive_type = SDL_GPUPrimitiveType.SDL_GPU_PRIMITIVETYPE_TRIANGLELIST,
                    target_info = new SDL_GPUGraphicsPipelineTargetInfo
                    {
                        color_target_descriptions = &colourTargetDesc,
                        num_color_targets = 1,

                    },
                    rasterizer_state = rasteriserState
                    //props = CreateProperty(SDL_PROP_GPU_GRAPHICSPIPELINE_CREATE_NAME_STRING, graphicsPipelineDescription.Name),
                };

                Handle = SDL_CreateGPUGraphicsPipeline(device.Handle, createInfo);
            }
        }

        Debug.Assert(Handle != IntPtr.Zero);
    }

    protected override void FreeResource()
    {
        SDL_ReleaseGPUGraphicsPipeline(Device.Handle, Handle);
    }

    private static SDL_GPUVertexElementFormat MapToSdl3GpuVertexElementFormat(RhiVertexAttributeFormat f) => (f) switch
    {
        RhiVertexAttributeFormat.Float => SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_FLOAT,
        RhiVertexAttributeFormat.Vector2 => SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_FLOAT2,
        RhiVertexAttributeFormat.Vector3 => SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_FLOAT3,
        RhiVertexAttributeFormat.Vector4 => SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_FLOAT4,
        RhiVertexAttributeFormat.R8G8B8A8_UNorm => SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_UBYTE4_NORM,
        _ => throw new NotSupportedException()
    };

    private static uint MapToSize(RhiVertexAttributeFormat f) => (f) switch
    {
        RhiVertexAttributeFormat.Float   => 4,
        RhiVertexAttributeFormat.Vector2 => 8,
        RhiVertexAttributeFormat.Vector3 => 12,
        RhiVertexAttributeFormat.Vector4 => 16,
        RhiVertexAttributeFormat.R8G8B8A8_UNorm => 4,
        _ => throw new NotSupportedException()
    };

    private static SDL_GPUBlendFactor MapBlendFactor(RhiBlendFactor bf) => bf switch
    {
        RhiBlendFactor.Zero => SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_ZERO,
        RhiBlendFactor.One => SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_ONE,
        RhiBlendFactor.SourceAlpha => SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_SRC_ALPHA,
        RhiBlendFactor.InverseSourceAlpha => SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_ONE_MINUS_SRC_ALPHA,
        _ => throw new NotImplementedException(),
    };

    private static SDL_GPUBlendOp MapBlendOp(RhiBlendOperation op) => op switch
    {
        RhiBlendOperation.Add => SDL_GPUBlendOp.SDL_GPU_BLENDOP_ADD,
        RhiBlendOperation.Subtract => SDL_GPUBlendOp.SDL_GPU_BLENDOP_SUBTRACT,
        _ => throw new NotImplementedException(),
    };

    private static SDL_GPUColorComponentFlags MapColourWriteMask(RhiColourMask mask)
    {
        SDL_GPUColorComponentFlags flags = 0;

        if ((mask & RhiColourMask.R) != 0)
        {
            flags |= SDL_GPUColorComponentFlags.SDL_GPU_COLORCOMPONENT_R;
        }

        if ((mask & RhiColourMask.G) != 0)
        {
            flags |= SDL_GPUColorComponentFlags.SDL_GPU_COLORCOMPONENT_G;
        }

        if ((mask & RhiColourMask.B) != 0)
        {
            flags |= SDL_GPUColorComponentFlags.SDL_GPU_COLORCOMPONENT_B;
        }

        if ((mask & RhiColourMask.A) != 0)
        {
            flags |= SDL_GPUColorComponentFlags.SDL_GPU_COLORCOMPONENT_A;
        }

        return flags;
    }
}