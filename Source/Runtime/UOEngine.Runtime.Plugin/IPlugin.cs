// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Microsoft.Extensions.DependencyInjection;

namespace UOEngine.Runtime.Plugin;

public enum PluginLoadingPhase
{
    Runtime,
    Default
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class PluginDependencyAttribute : Attribute
{
    public Type Dependency { get; }
    public PluginDependencyAttribute(Type dependency) => Dependency = dependency;
}

public interface IPlugin
{
    public PluginLoadingPhase Priority => PluginLoadingPhase.Default;

    // Default empty implementation so no need to have to implement explicitly if not needed.
    static void ConfigureServices(IServiceCollection services){}

    public void Startup(){}
    public void PostStartup() { }
    public void Shutdown() {}
}
