// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Microsoft.Extensions.DependencyInjection;

using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;

using UOEngine.Developer.RenderDoc;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.Platform;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.Renderer;
using UOEngine.Runtime.Vulkan;

namespace UOEngine.Runtime.Application;

public class UOEngineApplication : IDisposable
{
    public static int FrameNumber { get; private set; }

    private readonly ServiceCollection _services = new ServiceCollection();
    private ServiceProvider? _serviceProvider;
    private ServiceProvider Services => _serviceProvider ?? throw new InvalidOperationException("Not initialized.");

    private ApplicationLoop _applicationLoop = null!;

    private RenderSystem _renderSystem = null!;

    private Window _window = new();
    private PlatformEventLoop _platformEventLoop = null!;

    private readonly Assembly[] _loadedAssemblies = [];

    private readonly PluginRegistry _pluginRegistry = new();

    private bool _disposed = false;

    public UOEngineApplication()
    {
        AppDomain.CurrentDomain.FirstChanceException += (_, e) =>
        {
            if (e.Exception is DllNotFoundException dllEx)
            {
                Debug.WriteLine($"DllNotFound: {dllEx.Message} (DllName: {dllEx.GetType().GetProperty("DllName")?.GetValue(dllEx) ?? "n/a"})");
            }
        };

        _loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        AssemblyLoadContext.Default.Resolving += (context, name) =>
        {
            string path = Path.Combine(AppContext.BaseDirectory, $"{name.Name}.dll");
            return File.Exists(path)
                ? context.LoadFromAssemblyPath(path)
                : null;
        };
    }

    internal void Start()
    {
        InitialiseInternal();
        OnInitialisationCompleted();

        float deltaSeconds = 0.0f;

        var platformEventLoop = GetService<ApplicationLoop>();

        while (platformEventLoop.ExitRequested == false)
        {
            Update(deltaSeconds);

            _renderSystem.FrameBegin();
            _renderSystem.FrameEnd();
            //TODO, should sleep based on frame time.
        }

        _window.Dispose();

        var plugins = Services.GetServices<IPlugin>();

        foreach (var plugin in plugins)
        {
            plugin.Shutdown();
        }
    }

    public T GetService<T>() where T : notnull
    {
        return Services.GetRequiredService<T>();
    }

    public void Dispose()
    {
        Dispose(disposing: true);

        GC.SuppressFinalize(this);
    }

    protected virtual void OnInitialisationCompleted()
    {
    }

    private void InitialiseInternal()
    {
        PluginRegistrationExtensions.Initialise(_pluginRegistry);

        // Some basic defaults.
        _services.AddSingleton<ApplicationLoop>();
        _services.AddSingleton<InputManager>();
        _services.AddSingleton<IWindow>(_window);
        _services.AddSingleton<PlatformEventLoop>();

        _pluginRegistry.LoadPlugin<RendererPlugin>();
        _pluginRegistry.LoadPlugin<VulkanPlugin>();

        if (CommandLine.HasOption("-renderdoc"))
        {
            _services.AddSingleton<RenderDocPlugin>();
        }

        //_pluginRegistry.LoadPlugins(UOEPaths.ExeDir);
        //_pluginRegistry.LoadPlugins(UOEPaths.PluginDir, true);

        _pluginRegistry.Build(_services);

        _serviceProvider = _services.BuildServiceProvider();

        _window.Startup(GetService<PlatformEventLoop>());

        var plugins = _serviceProvider.GetServices<IPlugin>();

        foreach (var plugin in plugins)
        {
            plugin.Startup();
        }

        foreach (var plugin in plugins)
        {
            plugin.PostStartup();
        }

        _applicationLoop = GetService<ApplicationLoop>();
        _renderSystem = GetService<RenderSystem>();
        _platformEventLoop = GetService<PlatformEventLoop>();

        _platformEventLoop.OnQuit += () =>
        {
            _applicationLoop.RequestExit("Platform event quit");
        };

        _window.OnResized += (window) =>
        {
            _renderSystem.ResizeSwapchain(window.Width, window.Height);
        };
    }

    private void Update(float gameTime)
    {
        if (_platformEventLoop.PollEvents())
        {
            return;
        }

        _applicationLoop.Update(gameTime);

        FrameNumber++;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed == false)
        {
            if (disposing)
            {
            }

            _disposed = true;
        }
    }

    private bool IsAssemblyLoaded(string path)
    {
        var loadedAssembly = _loadedAssemblies.FirstOrDefault(p =>
        {
            return p.Location == path;
        });

        return loadedAssembly != null;
    }


}
