// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;
using System.Reflection;
using System.Text;

using Microsoft.Extensions.DependencyInjection;

namespace UOEngine.Runtime.Plugin;

public class PluginRegistry
{
    private readonly HashSet<Type> _types = [];
    private readonly Queue<Type> _configureQueue = [];

    [DebuggerDisplay("{Type.Name} ({Phase.ToString()})")]
    private sealed record PluginDescriptor(Type Type, IReadOnlyList<Type> Dependencies, PluginLoadingPhase Phase);

    private readonly List<PluginDescriptor> _pluginDescriptors = [];

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
        var loadingPhase = pluginType.GetCustomAttribute<PluginLoadingPhaseAttribute>()?.Phase ?? PluginLoadingPhase.Default;
        var shouldLoad = pluginType.GetCustomAttribute<DisablePluginAttribute>() == null;

        if(shouldLoad == false)
        {
            Console.WriteLine($"Not loading {nameof(pluginType)} as has DisablePlugin attribute.");

            return false;
        }

        _configureQueue.Enqueue(pluginType);

        _pluginDescriptors.Add(new PluginDescriptor(pluginType, dependencies, loadingPhase));

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

        var entryTypes = types.Where(type => type.GetCustomAttribute<PluginEntryAttribute>() != null).ToArray();

        if(entryTypes.Length == 0)
        {
            Console.WriteLine($"No [PluginEntry] type found in {assembly.FullName}");

            return;
        }

        if(entryTypes.Length > 1)
        {
            Console.WriteLine($"Multiple [PluginEntry] types found in {assembly.FullName}: {string.Join(",", entryTypes.Select(t => t.FullName))}");

            return;
        }

        var entryType = entryTypes[0];

        if(typeof(IPlugin).IsAssignableFrom(entryType) == false || entryType.IsAbstract)
        {
            Console.WriteLine($"[PluginEntry] type {entryType.FullName} is not a non-abstract IPlugin.");

            return;
        }

        if (LoadPlugin(entryType))
        {
            Console.WriteLine($"Added plugin {pluginAssemblyPath} for registration.");
        }

    }

    public void Build(ServiceCollection services)
    {
        while(_configureQueue.Count > 0)
        {
            var type = _configureQueue.Dequeue();

            AutoRegisterServices(type.Assembly, services);

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

        // Get plugins with no dependencies as they are ready.
        var ready = incoming.Where(kv => kv.Value.Count == 0).Select(kv => kv.Key).ToList();

        while (ready.Count > 0)
        {
            ready.Sort((left, right) => 
            {
                return (int)left.Phase - (int)right.Phase;
            });

            var next = ready[0];

            ready.RemoveAt(0);

            yield return next;

            // Dependencies loaded?
            foreach (var kv in incoming.Where(kv => kv.Value.Remove(next.Type)).ToList())
            {
                // If so, it is ready.
                if (kv.Value.Count == 0)
                {
                    ready.Add(kv.Key);
                }
            }
        }

        if (incoming.Any(kv => kv.Value.Count > 0))
        {
            throw new InvalidOperationException("Plugin dependency cycle detected.");
        }
    }

    private static void AutoRegisterServices(Assembly assembly, IServiceCollection services)
    {
        Type[] types;

        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.Where(t => t != null).Cast<Type>().ToArray();
        }

        foreach (var type in types)
        {
            if (!type.IsClass || type.IsAbstract)
            {
                continue;
            }

            foreach (var attr in type.GetCustomAttributes<ServiceAttribute>())
            {
                var serviceType = attr.ServiceType ?? type;
                services.Add(new ServiceDescriptor(serviceType, type, attr.Lifetime));
            }
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
