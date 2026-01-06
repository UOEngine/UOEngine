// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.Loader;

using UOEngine.Runtime.Core;
using UOEngine.Runtime.Platform;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.Renderer;
using UOEngine.Runtime.Vulkan;

namespace UOEngine.Runtime.Application;

public sealed class UOEngineAppBuilder
{
    public Type? ApplicationType { get; private set; }

    private readonly ServiceCollection _services = new();
    private readonly PluginRegistry _pluginRegistry = new();

    public static UOEngineAppBuilder Configure<TApp>() where TApp: UOEngineApplication
    {
        if (CommandLine.HasOption("-wait_for_debugger"))
        {
            UOEDebug.WaitForDebugger();
        }

        AppDomain.CurrentDomain.FirstChanceException += (_, e) =>
        {
            if (e.Exception is DllNotFoundException dllEx)
            {
                string message = $"DllNotFound: {dllEx.Message} (DllName: {dllEx.GetType().GetProperty("DllName")?.GetValue(dllEx) ?? "n/a"})";

                UOEDebug.Assert(false, message);
            }
        };

        AssemblyLoadContext.Default.Resolving += (context, name) =>
        {
            string path = Path.Combine(AppContext.BaseDirectory, $"{name.Name}.dll");
            return File.Exists(path)
                ? context.LoadFromAssemblyPath(path)
                : null;
        };

        var builder = new UOEngineAppBuilder()
        {
            ApplicationType = typeof(TApp),
            //_appFactory = () => new TApp()
        };

        PluginRegistrationExtensions.Initialise(builder._pluginRegistry);

        return builder;
    }

    public UOEngineAppBuilder UseDefaults()
    {
        _services.AddSingleton<PlatformEventLoop>();
        _services.AddSingleton<ApplicationLoop>();
        _services.AddSingleton<InputManager>();
        _services.AddSingleton<IWindow, Window>();

        _services.AddSingleton(ApplicationType!);
        _services.AddSingleton(sp => (UOEngineApplication)sp.GetRequiredService(ApplicationType!));

        _pluginRegistry.LoadPlugin<RendererPlugin>();
        _pluginRegistry.LoadPlugin<VulkanPlugin>();

        return this;
    }

    public UOEngineAppBuilder ConfigureServices(Action<IServiceCollection> configure)
    {
        configure?.Invoke(_services);

        return this;
    }

    public UOEngineAppBuilder ConfigurePlugins(Action<PluginRegistry> configure)
    {
        configure?.Invoke(_pluginRegistry);

        return this;
    }

    public int Start(string[] args)
    {
        _pluginRegistry.Build(_services);

        var provider = _services.BuildServiceProvider();

        provider.GetRequiredService<IWindow>();

        var plugins = provider.GetServices<IPlugin>();

        foreach (var plugin in plugins)
        {
            plugin.Startup();
        }

        foreach (var plugin in plugins)
        {
            plugin.PostStartup();
        }

        var window = provider.GetRequiredService<IWindow>();
        
        var applicationLoop = provider.GetRequiredService<ApplicationLoop>();
        var platformEventLoop = provider.GetRequiredService<PlatformEventLoop>();
        var renderSystem = provider.GetRequiredService<RenderSystem>();

        platformEventLoop.OnQuit += () =>
        {
            applicationLoop.RequestExit("Platform Event Quit");
        };

        var app = provider.GetRequiredService<UOEngineApplication>();

        app.Start();

        float deltaSeconds = 0.0f;

        while (applicationLoop.ExitRequested == false)
        {
            if(platformEventLoop.PollEvents())
            {
                break;
            }

            app.Update(deltaSeconds);

            renderSystem.FrameBegin();
            renderSystem.FrameEnd();
        }

        return 0;
    }
}
