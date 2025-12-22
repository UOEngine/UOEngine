using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.Platform;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.Renderer;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.FnaAdapter;

public class FnaAdapterPlugin: IPlugin
{
    public PluginLoadingPhase Priority => PluginLoadingPhase.Runtime;

    public static FnaAdapterPlugin Instance { get; private set; }

    // These are all public for the fake FNA classes to use what is required under the hood.
    public readonly IWindow Window;
    public readonly InputManager InputManager;

    public readonly IRenderResourceFactory RenderResourceFactory;

    private readonly Remapper _shaderRemapper;

    private readonly List<Game> _hostedFNAGames = [];

    public FnaAdapterPlugin(IWindow window, InputManager inputmanager, IRenderResourceFactory renderResourceFactory,
        IRenderDevice renderDevice, Remapper remapper, RenderSystem renderSystem, ApplicationLoop _applicationLoop)
    {
        Instance = this;

        // Grab what we need here to set things up throughout fake FNA.
        Window = window;
        InputManager = inputmanager;
        RenderResourceFactory = renderResourceFactory;
        _shaderRemapper = remapper;

        // Todo - below is dirty.
        _applicationLoop.OnUpdate += (float deltaTime) =>
        {
            foreach(var game in _hostedFNAGames)
            {
                game.Tick1();
            }
        };

        renderSystem.OnFrameBegin += (IRenderContext renderContext) =>
        {
            foreach (var game in _hostedFNAGames)
            {
                game.GraphicsDevice.OnFrameBegin(renderContext);
                game.Tick2();
            }
        };
    }

    public static void ConfigureServices(IServiceCollection services) 
    {
        services.AddSingleton<Remapper>();
    }

    public void PostStartup() 
    {
        _shaderRemapper.RemapEffect<SpriteEffect>(Path.Combine(UOEPaths.ShadersDir, @"FNACompat\SpriteEffect.hlsl"), [new Technique
        {
            Name = "SpriteBatch",
            Programs = [new TechniqueProgramEntry("SpriteVertexShader", "SpritePixelShader")]
        }], "SpriteEffect");

        string?[] vsArray =
{
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            "VSBasicTxVcNoFog",
        };

        int[] vsIndices =
        {
            0,      // basic
            1,      // no fog
            2,      // vertex color
            3,      // vertex color, no fog
            4,      // texture
            5,      // texture, no fog
            6,      // texture + vertex color
            7,      // texture + vertex color, no fog
        };

        string?[] psArray =
        {
            null,
            null,
            null,
            "PSBasicTxNoFog",
        };

        int[] psIndices =
        {
            0,      // basic
            1,      // no fog
            0,      // vertex color
            1,      // vertex color, no fog
            2,      // texture
            3,      // texture, no fog
            2,      // texture + vertex color
            3,      // texture + vertex color, no fog
        };

        Debug.Assert(vsIndices.Length == psIndices.Length);

        List<TechniqueProgramEntry> basicEffectPrograms = [];

        for (int i = 0; i < vsIndices.Length; i++)
        {
            string? vertexMain = vsArray[vsIndices[i]];
            string? pixelMain = psArray[psIndices[i]];

            if(vertexMain == null || pixelMain == null)
            {
                continue;
            }

            basicEffectPrograms.Add(new TechniqueProgramEntry(vertexMain, pixelMain));

        }

        _shaderRemapper.RemapEffect<BasicEffect>(Path.Combine(UOEPaths.ShadersDir, @"FNACompat\BasicEffect.hlsl"), [new Technique
        {
            Name = "BasicEffect",
            Programs = [.. basicEffectPrograms]
        }], "BasicEffect");
    }

    public void RegisterGame(Game game)
    {
        _hostedFNAGames.Add(game);
    }
}
