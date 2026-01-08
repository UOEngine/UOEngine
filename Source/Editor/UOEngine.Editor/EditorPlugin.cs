// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Microsoft.Extensions.DependencyInjection;

using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.RHI;
using UOEngine.Runtime.FnaAdapter;

using UOEngine.Ultima.UOAssets;

namespace UOEngine.Editor;

[PluginEntry]
[PluginLoadingPhase(PluginLoadingPhase.Default)]
internal class UO3DApplication : IPlugin
{
    public UO3DApplication(IServiceProvider serviceProvider)
    {
    }

    public void PostStartup()
    {
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<UOAssetLoader>();
        services.AddPlugin<FnaAdapterPlugin>();
    }

    public void OnFrameBegin(IRenderContext context)
    {

    }

}
