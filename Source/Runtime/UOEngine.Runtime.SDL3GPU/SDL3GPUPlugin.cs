// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Microsoft.Extensions.DependencyInjection;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.SDL3GPU;

public class SDL3GPUPlugin: IPlugin
{
    private readonly SDL3GPURenderer _renderer;

    public SDL3GPUPlugin(IRenderer renderer)
    {
        _renderer = (SDL3GPURenderer)renderer;
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IRenderer, SDL3GPURenderer>();
        services.AddSingleton<IRenderResourceFactory, SDL3GPUResourceFactory>();
        services.AddSingleton<Sdl3GpuDevice>();
    }

    public void Startup()
    {
        _renderer.Startup();
    }

    public void Shutdown()
    {
    }
}
