// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
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
        UOEThread.Init();

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

        var stopWatch = Stopwatch.StartNew();
        long lastTicks = stopWatch.ElapsedTicks;

        var process = Process.GetCurrentProcess();

        long OneKb = 1024;
        long OneMb = OneKb * 1024;

        long totalMemoryLimit = OneMb * 4096;

        using var cts = new CancellationTokenSource();

        long latestWorkingSet = process.WorkingSet64;

        var memoryTask = Task.Run(async () =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                process.Refresh();
                Interlocked.Exchange(ref latestWorkingSet, process.WorkingSet64);

                try
                {
                    await Task.Delay(500, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        });

        while (applicationLoop.ExitRequested == false)
        {
            long now = stopWatch.ElapsedTicks;
            long deltaTicks = now - lastTicks;
            
            lastTicks = now;

            float deltaSeconds = (float)deltaTicks / Stopwatch.Frequency;

            if (platformEventLoop.PollEvents())
            {
                break;
            }

            applicationLoop.Update(deltaSeconds);

            app.Update(deltaSeconds);

            renderSystem.FrameBegin();
            renderSystem.FrameEnd();

            if (latestWorkingSet >= totalMemoryLimit)
            {
                // This is just temp to make sure if I leak memory, I don't crash my computer. 
                // Had native leaks before due to allocating lots of buffers without freeing, ops.
                Debug.WriteLine($"{latestWorkingSet / (1024 * 1025)} MB in use, max {totalMemoryLimit / (1024 * 1024)}");

                renderSystem.PrintStats();

                if(Debugger.IsAttached == false)
                {
                    throw new OutOfMemoryException("Ran out of memory!");
                }
            }

        }

        cts.Cancel();

        try
        {
            memoryTask.Wait();
        }
        catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
        {
        }

        return 0;
    }
}
