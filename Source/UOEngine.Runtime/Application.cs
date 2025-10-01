using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.Xna.Framework;

using UOEngine.Plugin;

namespace UOEngine.Runtime;

public class Application: Game
{
    public static string? ExePath => Assembly.GetEntryAssembly().Location;

    public static string? BaseDirectory => Path.GetDirectoryName(ExePath);

    private readonly ServiceCollection _services = new ServiceCollection();
    private ServiceProvider? _serviceProvider;

    public void RegisterPlugin<T>() where T : IPlugin
    {
        _services.AddSingleton(typeof(IPlugin), typeof(T));
    }

    public void Start()
    {
        new GraphicsDeviceManager(this);

        Run();
    }

    protected override void Initialize()
    {
        LoadPlugins();

        _serviceProvider = _services.BuildServiceProvider();

        var plugins = _serviceProvider.GetServices<IPlugin>();

        foreach (var plugin in plugins)
        {
            plugin.Startup();
        }

        base.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        base.Draw(gameTime);
    }

    private void LoadPlugins()
    {
        foreach (var dll in Directory.GetFiles(BaseDirectory, "*.dll"))
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

                _services.AddSingleton(typeof(IPlugin), type);
            }
        }
    }
}
