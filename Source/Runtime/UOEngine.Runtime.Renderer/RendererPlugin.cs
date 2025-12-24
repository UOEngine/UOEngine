// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Plugin;

namespace UOEngine.Runtime.Renderer;

[PluginEntry]
public class RendererPlugin : IPlugin
{
    private readonly RenderSystem _renderSystem;

    public RendererPlugin(RenderSystem renderSystem)
    {
        _renderSystem = renderSystem;
    }

    public void PostStartup()
    {
        _renderSystem.Startup();
    }
}
