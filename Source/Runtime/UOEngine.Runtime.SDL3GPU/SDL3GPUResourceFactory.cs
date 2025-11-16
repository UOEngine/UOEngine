using static SDL3.SDL;

using UOEngine.Runtime.RHI;
using UOEngine.Runtime.RHI.Resources;

using UOEngine.Runtime.SDL3GPU.Resources;

namespace UOEngine.Runtime.SDL3GPU;

internal class SDL3GPUResourceFactory : IRenderResourceFactory
{
    //private readonly IntPtr _device;
    private readonly Sdl3GpuDevice _device;

    public SDL3GPUResourceFactory(Sdl3GpuDevice device)
    {
        _device = device;
    }

    public RhiShaderResource NewShaderResource()
    {
        return new Sdl3GpuShaderResource(_device);
    }

    public ShaderInstance NewShaderInstance(RhiShaderResource shaderResource)
    {
        return new ShaderInstance(shaderResource);
    }

    public IRenderTexture CreateTexture(in RenderTextureDescription description)
    {
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

    public IGraphicsPipeline CreateGraphicsPipeline(in GraphicsPipelineDescription graphicsPipelineDescription)
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
        var vertexBuffer = new Sdl3GpuVertexBuffer(description);

        return vertexBuffer;
    }
}
