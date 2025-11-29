using System.Reflection;
using CentrED;
using CentrED.Client;
using CentrED.Server;
using CentrED.Utils;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.Platform;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.RHI;

using CentredApplication = CentrED.Application;

namespace UOEngine.Editor.CentredSharp;

[PluginDependency(typeof(FnaAdapterPlugin))]
public class CentrEdSharpPlugin: IPlugin
{
    static public string WorkDir { get; } = AppContext.BaseDirectory;

    public static CentrEDGame CEDGame { get; private set; } = null!;
    public static CEDServer? CEDServer;
    public static readonly CentrEDClient CEDClient = new();
    public static readonly Metrics Metrics = new();

    private readonly IRenderResourceFactory _renderResourceFactory;
    private readonly IWindow _window;
    private readonly IServiceProvider _serviceProvider;
    private readonly Remapper _shaderRemapper;
    private readonly FnaAdapterPlugin _fnaCompatPlugin;
    public CentrEdSharpPlugin(IRenderResourceFactory renderResourceFactory, IWindow window,
        IServiceProvider serviceProvider, Remapper shaderRemapper, FnaAdapterPlugin fnaCompatPlugin)
    {
        _renderResourceFactory = renderResourceFactory;
        _window = window;
        _serviceProvider = serviceProvider;
        _shaderRemapper = shaderRemapper;
        _fnaCompatPlugin = fnaCompatPlugin;
    }

    public void PostStartup() 
    {
        string mapEffectNew = @"D:\UODev\UOEngineGithub\Source\Shaders\CentrEdSharp\MapEffect.hlsl";
        string mapEffect = @"D:\UODev\UOEngineGitHub\ThirdParty\centredsharp\CentrED\Renderer\Shaders\MapEffect.fxc";

        var terrainTechnique = new Technique
        {
            Name = "Terrain",
            VertexMain = "TileVSMain",
            PixelMain = "TerrainPSMain"
        };

        var terrainGrid = new Technique
        {
            Name = "TerrainGrid",
            VertexMain = "TerrainGridVSMain",
            PixelMain = "TerrainGridPSMain"
        };

        var statics = new Technique
        {
            Name = "Statics",
            VertexMain = "TileVSMain",
            PixelMain = "StaticsPSMain"
        };

        var selection = new Technique
        {
            Name = "Selection",
            VertexMain = "TileVSMain",
            PixelMain = "SelectionPSMain"
        };

        var virtualLayer = new Technique
        {
            Name = "VirtualLayer",
            VertexMain = "VirtualLayerVSMain",
            PixelMain = "VirtualLayerPSMain"
        };

        _shaderRemapper.RemapTechniques(mapEffect, mapEffectNew,
        [
            terrainTechnique,
            terrainGrid,
            statics,
            selection,
            virtualLayer
        ]);

        Config.Initialize();

        CentrEDGame.PreSetup(_serviceProvider);

        CEDGame = new CentrEDGame();

        CentredApplication.SetFromUOEngine(CEDGame);

        _fnaCompatPlugin.RegisterGame(CEDGame);
        
    }

    public void Shutdown()
    {
        CEDGame.Dispose();
    }
}
