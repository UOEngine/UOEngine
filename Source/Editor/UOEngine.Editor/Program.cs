// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Editor;
using UOEngine.Editor.CentredSharp;

using UOEngine.Runtime.Application;
using UOEngine.Runtime.FnaAdapter;

public static class Program
{
    public static int Main(string[] args)
    {
        return UOEngineAppBuilder.Configure<UOEngineEditor>()
            .UseDefaults()
            .ConfigurePlugins(pluginRegistry =>
            {
                pluginRegistry.LoadPlugin<FnaAdapterPlugin>();
                pluginRegistry.LoadPlugin<CentrEdSharpPlugin>();
            })
            .Start(args);
    }
}
