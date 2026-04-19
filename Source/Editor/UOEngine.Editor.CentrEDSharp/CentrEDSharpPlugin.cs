// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Microsoft.Extensions.DependencyInjection;

using CentrED;
using CentrED.Client;
using CentrED.Server;
using CentrED.Utils;

using UOEngine.Editor.Abstractions;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.Plugin;

using CentredApplication = CentrED.Application;
using UOEngine.Runtime.Platform;

namespace UOEngine.Editor.CentredSharp;

[PluginEntry]
[PluginDependency(typeof(FnaAdapterPlugin))]
[PluginLoadingPhase(PluginLoadingPhase.Default)]
public class CentrEdSharpPlugin: IPlugin
{
    static public string WorkDir { get; } = AppContext.BaseDirectory;

    public static CentrEDGame CEDGame { get; private set; } = null!;
    public static CEDServer? CEDServer;
    public static readonly CentrEDClient CEDClient = new();
    public static readonly Metrics Metrics = new();

    private readonly IServiceProvider _serviceProvider;
    private readonly Remapper _shaderRemapper;

    public CentrEdSharpPlugin(IServiceProvider serviceProvider, Remapper shaderRemapper)
    {
        _serviceProvider = serviceProvider;
        _shaderRemapper = shaderRemapper;
    }

    public void PostStartup() 
    {
        string mapEffectNew = Path.Combine(UOEPaths.ShadersDir, @"CentrEdSharp\MapEffect.hlsl");
        string mapEffect = Path.Combine(UOEPaths.ProjectDir, @"ThirdParty\centredsharp\CentrED\Renderer\Shaders\MapEffect.fxc");

        var terrainTechnique = new Technique
        {
            Name = "Terrain",
            Programs = [new TechniqueProgramEntry("TileVSMain", "TerrainPSMain")]
        };

        var terrainGrid = new Technique
        {
            Name = "TerrainGrid",
            Programs = [new TechniqueProgramEntry("TerrainGridVSMain", "TerrainGridPSMain")]
        };

        var statics = new Technique
        {
            Name = "Statics",
            Programs = [new TechniqueProgramEntry("TileVSMain", "StaticsPSMain")]
        };

        var selection = new Technique
        {
            Name = "Selection",
            Programs = [new TechniqueProgramEntry("TileVSMain", "SelectionPSMain")]
        };

        var virtualLayer = new Technique
        {
            Name = "VirtualLayer",
            Programs = [new TechniqueProgramEntry("VirtualLayerVSMain", "VirtualLayerPSMain")]
        };

        _shaderRemapper.RemapTechniques(mapEffect, mapEffectNew,
        [
            terrainTechnique,
            terrainGrid,
            statics,
            selection,
            virtualLayer
        ], "MapEffect");

        Config.Initialize();
    }

    public void Shutdown()
    {
        CEDGame.Dispose();
    }
}
