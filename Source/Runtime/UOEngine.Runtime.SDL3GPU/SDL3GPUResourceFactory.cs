using static SDL3.SDL;

using UOEngine.Runtime.RHI;
using UOEngine.Runtime.RHI.Resources;

using UOEngine.Runtime.SDL3GPU.Resources;
using System.Runtime.CompilerServices;

namespace UOEngine.Runtime.SDL3GPU;

internal class SDL3GPUResourceFactory : IRenderResourceFactory
{
    //private readonly IntPtr _device;
    private readonly Sdl3GpuDevice _device;

    public SDL3GPUResourceFactory(IRenderDevice device)
    {
        _device = (Sdl3GpuDevice)device;
    }

    public RhiShaderResource NewShaderResource()
    {
        return new Sdl3GpuShaderResource(_device);
    }

    public ShaderInstance NewShaderInstance(RhiShaderResource shaderResource)
    {
        return new ShaderInstance(shaderResource);
    }

    public IRenderTexture CreateTexture(in RhiTextureDescription description)
    {
        SDL_GPUTextureUsageFlags gpuUsage;

        switch(description.Usage)
        {
            case RhiRenderTextureUsage.Sampler: gpuUsage = SDL_GPUTextureUsageFlags.SDL_GPU_TEXTUREUSAGE_SAMPLER; break;
            case RhiRenderTextureUsage.ColourTarget: gpuUsage = SDL_GPUTextureUsageFlags.SDL_GPU_TEXTUREUSAGE_COLOR_TARGET; break;
            default:
                throw new SwitchExpressionException("Unhandled usage type");
        }

        var texture = new SDL3GPUTexture(_device, new SDL3GPUTextureDescription
        {
            CreateInfo = new SDL_GPUTextureCreateInfo
            {
                width = description.Width,
                height = description.Height,
                format = SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_R8G8B8A8_UNORM,
                usage = SDL_GPUTextureUsageFlags.SDL_GPU_TEXTUREUSAGE_SAMPLER
            },
            Name = description.Name,
        });

        texture.Init();

        return texture;
    }

    public IGraphicsPipeline CreateGraphicsPipeline(in RhiGraphicsPipelineDescription graphicsPipelineDescription)
    {
        var pipeline = new Sdl3GpuGraphicsPipeline(_device, graphicsPipelineDescription);

        return pipeline;
    }

    public IRhiIndexBuffer CreateIndexBuffer(uint length, string name)
    {
        var indexBuffer = new Sdl3GpuIndexBuffer(_device, length, name);

        return indexBuffer;
    }

    public IRhiVertexBuffer CreateVertexBuffer(in RhiVertexBufferDescription description)
    {
        var vertexBuffer = new Sdl3GpuVertexBuffer(_device, description);

        return vertexBuffer;
    }
}
