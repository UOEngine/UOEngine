using System.Reflection;
using CentrED;
using CentrED.Client;
using CentrED.Server;
using CentrED.Utils;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.Plugin;

namespace UOEngine.Editor.CentredSharp;

public class CentrEdSharpPlugin: IPlugin
{
    static public string WorkDir { get; } = AppContext.BaseDirectory;

    public static CentrEDGame CEDGame { get; private set; } = null!;
    public static CEDServer? CEDServer;
    public static readonly CentrEDClient CEDClient = new();
    public static readonly Metrics Metrics = new();

    public CentrEdSharpPlugin()
    {
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

        Remapper.RemapTechniques(mapEffect, mapEffectNew,
        [
            terrainTechnique, 
            terrainGrid,
            statics,
            selection,
            virtualLayer
        ]);

        Config.Initialize();

        CEDGame = new CentrEDGame();

        CEDGame.DoInitialise();
    }

    public void Shutdown()
    {
        CEDGame.Dispose();
    }
}
