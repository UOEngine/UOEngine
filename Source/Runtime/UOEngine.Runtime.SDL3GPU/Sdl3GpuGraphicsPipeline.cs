using System.Diagnostics;

using static SDL3.SDL;

using UOEngine.Runtime.SDL3GPU.Resources;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.SDL3GPU;

internal class Sdl3GpuGraphicsPipeline: Sdl3GpuResource, IGraphicsPipeline
{
    public readonly SDL3GPUShaderProgram VertexProgram;
    public readonly SDL3GPUShaderProgram PixelProgram;

    public Sdl3GpuGraphicsPipeline(Sdl3GpuDevice device, in GraphicsPipelineDescription graphicsPipelineDescription)
        : base(device)
    {
        Sdl3GpuShaderResource shaderResource = (Sdl3GpuShaderResource)graphicsPipelineDescription.ShaderResource;

        VertexProgram = shaderResource!.VertexProgram;
        PixelProgram = shaderResource.PixelProgram; ;

        SDL_GPUColorTargetDescription colourTargetDesc = new SDL_GPUColorTargetDescription
        {
            format = SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_B8G8R8A8_UNORM,
        };

        SDL_GPUVertexAttribute[] vertexAttributes = new SDL_GPUVertexAttribute[VertexProgram.StreamBindings.Length];

        SDL_GPUVertexBufferDescription[] vertexBufferDescription = [];


        uint offset = 0;

        for(var i = 0; i < vertexAttributes.Length; i++)
        {
            ref var attribute = ref vertexAttributes[i];

            attribute.buffer_slot = 0; // Assume slot 0 for now.
            attribute.location = VertexProgram.StreamBindings[i].SemanticIndex;
            attribute.offset = offset;
            attribute.format = MapToSdl3GpuVertexElementFormat(VertexProgram.StreamBindings[i].Format);

            offset += MapToSize(VertexProgram.StreamBindings[i].Format);
        }

        if (vertexAttributes.Length > 0)
        {
            // Todo: Assume no instances for now.
            // We draw instanced with no vertex buffer as usually not required for quads.
            vertexBufferDescription = new SDL_GPUVertexBufferDescription[1];

            vertexBufferDescription[0].pitch = offset;
            vertexBufferDescription[0].instance_step_rate = 0;
        }

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
                    rasterizer_state = new SDL_GPURasterizerState
                    {
                        cull_mode = SDL_GPUCullMode.SDL_GPU_CULLMODE_BACK,
                        fill_mode = SDL_GPUFillMode.SDL_GPU_FILLMODE_FILL,
                        front_face = SDL_GPUFrontFace.SDL_GPU_FRONTFACE_CLOCKWISE
                    },
                    props = CreateProperty(SDL_PROP_GPU_GRAPHICSPIPELINE_CREATE_NAME_STRING, graphicsPipelineDescription.Name),
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
        _ => throw new NotSupportedException()
    };

    private static uint MapToSize(RhiVertexAttributeFormat f) => (f) switch
    {
        RhiVertexAttributeFormat.Float   => 4,
        RhiVertexAttributeFormat.Vector2 => 8,
        RhiVertexAttributeFormat.Vector3 => 12,
        RhiVertexAttributeFormat.Vector4 => 16,
                                       _ => throw new NotSupportedException()
    };
}
