// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.SDL3GPU;

[PluginEntry]
[PluginLoadingPhase(PluginLoadingPhase.Runtime)]
[DisablePlugin]
public class SDL3GPUPlugin: IPlugin
{
    private readonly SDL3GPURenderer _renderer;

    public SDL3GPUPlugin(IRenderer renderer)
    {
        _renderer = (SDL3GPURenderer)renderer;
    }

    public void Startup()
    {
        _renderer.Startup();
    }

    public void Shutdown()
    {
    }
}
