// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.Plugin;

public enum PluginLoadingPhase
{
    EarliestPossible,
    Runtime,
    PreDefault,
    Default,
    PostDefault,
    DoNotLoadAutomatically,
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class PluginLoadingPhaseAttribute : Attribute
{
    public PluginLoadingPhase Phase { get; }
    public PluginLoadingPhaseAttribute(PluginLoadingPhase phase) => Phase = phase;
}
