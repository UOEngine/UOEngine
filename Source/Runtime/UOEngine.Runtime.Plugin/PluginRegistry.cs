using System.Reflection;
using System.Text;

using Microsoft.Extensions.DependencyInjection;

namespace UOEngine.Runtime.Plugin;

public class PluginRegistry
{
    private readonly HashSet<Type> _types = [];
    private readonly Queue<Type> _configureQueue = [];

    private sealed record PluginDescriptor(Type Type, IReadOnlyList<Type> Dependencies);
    private readonly List<PluginDescriptor> _pluginDescriptors = new();

    public bool LoadPlugin(Type pluginType)
    {
        if (!typeof(IPlugin).IsAssignableFrom(pluginType) || pluginType.IsAbstract || !pluginType.IsClass)
        {
            throw new ArgumentException("Type must be a non-abstract IPlugin", nameof(pluginType));
        }

        if (_types.Add(pluginType) == false)
        {
            return false;
        }

        var dependencies = pluginType.GetCustomAttributes<PluginDependencyAttribute>().Select(a => a.Dependency).ToList();

        _configureQueue.Enqueue(pluginType);

        _pluginDescriptors.Add(new PluginDescriptor(pluginType, dependencies));

        return true;
    }

    public bool LoadPlugin<T>() where T : IPlugin
    {
        var type = typeof(T);

        return LoadPlugin(type);
    }

    public void LoadPlugins(string directory, bool recurse = false)
    {
        if (Directory.Exists(directory) == false)
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
            if (Path.GetFileName(dll).StartsWith("UOEngine") == false)
            {
                continue;
            }

            LoadPlugin(dll);
        }
    }

    public void LoadPlugin(string pluginAssemblyPath)
    {
        Assembly assembly;

        Console.WriteLine($"Loading {pluginAssemblyPath}.");

        try
        {
            assembly = Assembly.LoadFrom(pluginAssemblyPath);
        }
        catch
        {
            Console.WriteLine($"Loading {pluginAssemblyPath} failed.");

            return;
        }

        Type[] types = null!;

        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Error loading types from {assembly.FullName}:");

            foreach (var t in ex.Types)
            {
                sb.AppendLine($"Type: {t?.FullName ?? "(null)"}");
            }

            foreach (var le in ex.LoaderExceptions)
            {
                sb.AppendLine($"LoaderException: {le?.Message ?? "None"}");
            }

            Console.WriteLine(sb.ToString());

            return;
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

            if (LoadPlugin(type))
            {
                Console.WriteLine($"Added plugin {pluginAssemblyPath} for registration.");
            }

            return;
        }
    }

    public void Build(ServiceCollection services)
    {
        while(_configureQueue.Count > 0)
        {
            var type = _configureQueue.Dequeue();
            var configure = type.GetMethod("ConfigureServices", BindingFlags.Public | BindingFlags.Static);

            if (configure == null)
            {
                configure = type.GetMethod("ConfigureServices", BindingFlags.NonPublic);

                if (configure != null)
                {
                    throw new InvalidOperationException("ConfigureServices is private, should be public.");
                }
            }

            configure?.Invoke(null, [services]);
            // If ConfigureServices calls services.AddPlugin<Other>(), AddPlugin queues it.
        }

        foreach (var descriptor in TopologicalSort(_pluginDescriptors))
        {
            // register the concrete type
            services.AddSingleton(descriptor.Type);

            // expose it as IPlugin without creating a second instance
            services.AddSingleton(typeof(IPlugin), sp => (IPlugin)sp.GetRequiredService(descriptor.Type));

            Console.WriteLine($"Registered plugin {descriptor.Type.Name}");
        }
    }

    private static IEnumerable<PluginDescriptor> TopologicalSort(IReadOnlyCollection<PluginDescriptor> descriptors)
    {
        var incoming = descriptors.ToDictionary(
            d => d,
            d => new HashSet<Type>(d.Dependencies));

        var lookup = descriptors.ToDictionary(d => d.Type);
        var ready = new Queue<PluginDescriptor>(incoming.Where(kv => kv.Value.Count == 0)
                                                        .Select(kv => kv.Key));

        while (ready.Count > 0)
        {
            var next = ready.Dequeue();
            yield return next;

            foreach (var kv in incoming.Where(kv => kv.Value.Remove(next.Type)).ToList())
            {
                if (kv.Value.Count == 0)
                {
                    ready.Enqueue(kv.Key);
                }
            }
        }

        if (incoming.Any(kv => kv.Value.Count > 0))
        {
            throw new InvalidOperationException("Plugin dependency cycle detected.");
        }
    }

}

public static class PluginRegistrationExtensions
{
    public static PluginRegistry CurrentRegistry => 
        _currentRegistry ?? throw new InvalidOperationException("PluginRegistry has not been initialised.");

    private static PluginRegistry? _currentRegistry;

    public static void Initialise(PluginRegistry registry)
    {
        _currentRegistry = registry;
    }

    public static void AddPlugin<T>(this IServiceCollection services) where T : class, IPlugin
    {
        CurrentRegistry.LoadPlugin<T>();
    }
}
