// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia;

using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.Renderer;

namespace UOEngine.Runtime.AvaloniaUI;

[PluginEntry]
[PluginLoadingPhase(PluginLoadingPhase.Runtime)]
public class AvaloniaUIPlugin: IPlugin
{
    private AvaloniaControl? _rootControl;

    public AvaloniaUIPlugin(RenderSystem renderSystem)
    {
        renderSystem.OnFrameEnd += (renderContext) =>
        {
            _rootControl!.Draw();
        };
    }

    public void PostStartup()
    {
        AppBuilder.Configure<AvaloniaApp>()
            .UseUOEngine()
            .SetupWithoutStarting();

        _rootControl = new AvaloniaControl();
        _rootControl.Initialise();
    }

}

public static class AppBuilderExtensions
{
    public static AppBuilder UseUOEngine(this AppBuilder builder)
        => builder
            .UseStandardRuntimePlatformSubsystem()
            .UseSkia()
            .UseWindowingSubsystem(UOEnginePlatform.Initialise);
}
