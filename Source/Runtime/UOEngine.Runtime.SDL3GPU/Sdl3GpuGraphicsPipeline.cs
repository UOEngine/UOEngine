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
            format = SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_B8G8R8A8_UNORM
        };

        unsafe
        {
            var createInfo = new SDL_GPUGraphicsPipelineCreateInfo
            {
                vertex_shader = VertexProgram.Handle,
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

        Debug.Assert(Handle != IntPtr.Zero);
    }

    protected override void FreeResource()
    {
        SDL_ReleaseGPUGraphicsPipeline(Device.Handle, Handle);
    }
}
