using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UOEngine.Plugin;

namespace UOEngine.Runtime;

public class Application: Game
{
    public static string? ExePath => Assembly.GetEntryAssembly().Location;

    public static string? BaseDirectory => Path.GetDirectoryName(ExePath);
    public static string? PluginDirectory => Path.Combine(BaseDirectory, "Plugins");

    private readonly ServiceCollection _services = new ServiceCollection();
    private ServiceProvider? _serviceProvider;

    private ApplicationLoop _applicationLoop = null!;
    private Renderer.Renderer _renderer = null!; 

    private GraphicsDeviceManager _graphicsDeviceManager = null!;

    public void RegisterPlugin<T>() where T : IPlugin
    {
        _services.AddSingleton(typeof(IPlugin), typeof(T));
    }

    public void Start()
    {
        _graphicsDeviceManager = new GraphicsDeviceManager(this);


        Run();
    }

    protected override void Initialize()
    {
        _services.AddSingleton<EntityManager>();
        _services.AddSingleton<ApplicationLoop>();
        _services.AddSingleton<Input>();


        _services.AddSingleton(typeof(GraphicsDevice), _graphicsDeviceManager.GraphicsDevice);

        LoadPlugins(BaseDirectory);
        LoadPlugins(PluginDirectory, true);

        _serviceProvider = _services.BuildServiceProvider();

        _applicationLoop = _serviceProvider.GetRequiredService<ApplicationLoop>();
        _renderer = _serviceProvider.GetRequiredService<Renderer.Renderer>();

        var plugins = _serviceProvider.GetServices<IPlugin>();

        foreach (var plugin in plugins)
        {
            Services.AddService(plugin.GetType(), plugin);

            plugin.Startup();
        }

        base.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        _applicationLoop.Update(gameTime.ElapsedGameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _renderer.Draw(gameTime.ElapsedGameTime);
    }

    private void LoadPlugins(string directory, bool recurse = false)
    {
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
            try
            {
                assembly = Assembly.LoadFrom(dll);
            }
            catch
            {
                continue; // skip invalid DLLs
            }

            foreach (var type in assembly.GetTypes())
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
}
