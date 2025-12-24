// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Microsoft.Extensions.DependencyInjection;

using UOEngine.Runtime.Plugin;

namespace UOEngine.Runtime.Renderer;

public class RendererPlugin : IPlugin
{
    private readonly RenderSystem _renderSystem;

    public RendererPlugin(RenderSystem renderSystem)
    {
        _renderSystem = renderSystem;
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<RenderSystem>();
    }

    public void PostStartup()
    {
        _renderSystem.Startup();
    }
}
