// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Input;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.Platform;
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
    private readonly IWindow _window;
    private readonly InputManager _inputManager;

    public AvaloniaUIPlugin(RenderSystem renderSystem, 
        IRenderer renderer, 
        IRootContentHost rootContentHost, 
        IWindow window,
        InputManager inputManager)
    {
        _renderer = renderer;
        _renderSystem = renderSystem;
        _rootContentHost = rootContentHost;
        _window = window;
        _inputManager = inputManager;

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

        _rootControl = new AvaloniaControl(_rootContentHost, _window, _inputManager);
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
