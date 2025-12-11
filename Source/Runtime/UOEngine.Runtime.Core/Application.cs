using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

using Microsoft.Extensions.DependencyInjection;

using UOEngine.Runtime.Platform;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.Renderer;

namespace UOEngine.Runtime.Core;

public class Application: IDisposable
{
    public static string? ExePath => Assembly.GetEntryAssembly().Location;

    public static string? BaseDirectory => Path.GetDirectoryName(ExePath);
    public static string? PluginDirectory => Path.Combine(BaseDirectory, "Plugins");

    private readonly ServiceCollection _services = new ServiceCollection();
    private ServiceProvider? _serviceProvider;

    private ApplicationLoop _applicationLoop = null!;

    private RenderSystem _renderSystem = null!; 

    private CameraEntity _camera = null!;

    private Window _window = new();
    private PlatformEventLoop _platformEventLoop = null!;

    private bool _runApplication = true;

    private readonly Assembly[] _loadedAssemblies = [];

    private readonly PluginRegistry _pluginRegistry = new();

    public Application()
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

    public void Start()
    {
        InitialiseInternal();
        //Initialise();

        float deltaSeconds = 0.0f;

        while (_runApplication)
        {
            Update(deltaSeconds);
            _renderSystem.FrameBegin();
            _renderSystem.FrameEnd();
        }

        _window.Dispose();

        var plugins = _serviceProvider.GetServices<IPlugin>();

        foreach (var plugin in plugins)
        {
            plugin.Shutdown();
        }
    }

    public T GetService<T>()
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    //virtual protected void Initialise()
    //{

    //}

    //virtual protected void BeginDraw(IRenderContext context) 
    //{
    //}

    //virtual protected void EndDraw(IRenderContext context)
    //{

    //}

    private void InitialiseInternal()
    {
        PluginRegistrationExtensions.CurrentRegistry = _pluginRegistry;

        _services.AddSingleton<EntityManager>();
        _services.AddSingleton<ApplicationLoop>();
        _services.AddSingleton<InputManager>();
        _services.AddSingleton<IWindow>(_window);
        _services.AddSingleton<PlatformEventLoop>();

        _pluginRegistry.LoadPlugins(BaseDirectory);
        _pluginRegistry.LoadPlugins(PluginDirectory, true);

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

        var entityManager = GetService<EntityManager>();

        _camera = entityManager.NewEntity<CameraEntity>();

        //_renderSystem.OnFrameBegin += BeginDraw;
        //_renderSystem.OnFrameEnd += EndDraw;

        _window.OnResized += (window) =>
        {
            _renderSystem.ResizeSwapchain(window.Width, window.Height);
        };
    }

    private void Update(float gameTime)
    {
        if(_platformEventLoop.PollEvents())
        {
            _runApplication = false;

            return;
        }

        _applicationLoop.Update(gameTime);
    }

    public void Dispose()
    {
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
