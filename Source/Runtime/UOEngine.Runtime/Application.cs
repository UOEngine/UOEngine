using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

using UOEngine.Runtime.Platform;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.Renderer;
using UOEngine.Runtime.RHI;

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

    private bool _runApplication = true;

    private readonly Assembly[] _loadedAssemblies = [];

    public Application()
    {
        _loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
    }

    public void RegisterPlugin<T>() where T : IPlugin
    {
        _services.AddSingleton(typeof(IPlugin), typeof(T));
    }

    public void Start()
    {
        InitialiseInternal();
        Initialise();

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

    virtual protected void Initialise()
    {

    }

    virtual protected void BeginDraw(IRenderContext context) 
    {
    }

    virtual protected void EndDraw(IRenderContext context)
    {

    }

    private void InitialiseInternal()
    {
        _window.Startup();

        _services.AddSingleton<EntityManager>();
        _services.AddSingleton<ApplicationLoop>();
        _services.AddSingleton<Input>();
        _services.AddSingleton<IWindow>(_window);

        LoadPlugins(BaseDirectory);
        LoadPlugins(PluginDirectory, true);

        _serviceProvider = _services.BuildServiceProvider();

        _applicationLoop = GetService<ApplicationLoop>();
        _renderSystem = GetService<RenderSystem>();

        var plugins = _serviceProvider.GetServices<IPlugin>();

        foreach (var plugin in plugins)
        {
            plugin.Startup();
        }

        foreach (var plugin in plugins)
        {
            plugin.PostStartup();
        }

        var entityManager = GetService<EntityManager>();

        _camera = entityManager.NewEntity<CameraEntity>();

        _renderSystem.OnFrameBegin += BeginDraw;
        _renderSystem.OnFrameEnd += EndDraw;

        _window.OnResized += (window) =>
        {
            _renderSystem.ResizeSwapchain(window.Width, window.Height);
        };
    }

    private void Update(float gameTime)
    {
        if(_window.PollEvents())
        {
            _runApplication = false;

            return;
        }

        _applicationLoop.Update(gameTime);
    }

    private void LoadPlugins(string directory, bool recurse = false)
    {
        if(Directory.Exists(directory) == false)
        {
            return;
        }

        if (recurse)
        {
            foreach (var subdir in Directory.GetDirectories(directory))
            {
                LoadPlugins(subdir, true);
            }
        }

        foreach (var dll in Directory.GetFiles(directory, "*.dll"))
        {
            Assembly assembly;

            if(Path.GetFileName(dll).StartsWith("UOEngine") == false)
            {
                continue;
            }

            try
            {
                assembly = Assembly.LoadFrom(dll);
            }
            catch
            {
                continue; // skip invalid DLLs
            }

            Console.WriteLine($"Loaded {dll}.");

            Type[] types = null!;

            try
            {
                types = assembly.GetTypes();
            }
            catch(ReflectionTypeLoadException ex)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Error loading types from {assembly.FullName}:");
                foreach (var t in ex.Types)
                    sb.AppendLine($"Type: {t?.FullName ?? "(null)"}");
                foreach (var le in ex.LoaderExceptions)
                    sb.AppendLine($"LoaderException: {le.Message}");
                Console.WriteLine(sb.ToString());
            }
            catch
            {
                continue;
            }

            foreach (var type in types)
            {
                if (typeof(IPlugin).IsAssignableFrom(type) == false)
                {
                    continue; // skip non-plugins
                }

                if (type.IsAbstract || !type.IsClass)
                {
                    continue;
                }

                Console.WriteLine($"Loading plugin {dll}");

                var configureServicesMethod = type.GetMethod("ConfigureServices");

                configureServicesMethod?.Invoke(null, [_services]);

                _services.AddSingleton(typeof(IPlugin), type);
            }
        }
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
