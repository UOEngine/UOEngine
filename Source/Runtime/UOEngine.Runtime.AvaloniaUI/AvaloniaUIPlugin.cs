// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia;

using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.Renderer;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.AvaloniaUI;

[PluginEntry]
[PluginLoadingPhase(PluginLoadingPhase.Runtime)]
public class AvaloniaUIPlugin: IPlugin
{
    private AvaloniaControl? _rootControl;
    private readonly IRenderer _renderer;
    private readonly RenderSystem _renderSystem;
    private readonly IRootContentHost _rootContentHost;

    public AvaloniaUIPlugin(RenderSystem renderSystem, IRenderer renderer, IRootContentHost rootContentHost)
    {
        _renderer = renderer;
        _renderSystem = renderSystem;
        _rootContentHost = rootContentHost;

        renderSystem.OnFrameEnd += (mainRenderContext) =>
        {
            _rootControl!.Draw(mainRenderContext);
        };
    }

    public void PostStartup()
    {
        AppBuilder.Configure<AvaloniaApp>()
            .LogToTrace()
            .UseUOEngine(_renderer, _renderSystem)
            .SetupWithoutStarting();

        _rootControl = new AvaloniaControl(_rootContentHost);
        _rootControl.Initialise();
    }
}

public static class AppBuilderExtensions
{
    public static AppBuilder UseUOEngine(this AppBuilder builder, IRenderer renderer, RenderSystem renderSystem)
        => builder
            .UseStandardRuntimePlatformSubsystem()
            .UseHarfBuzz()
            .UseSkia()
            .UseWindowingSubsystem(() => UOEnginePlatform.Initialise(renderer, renderSystem));
}
