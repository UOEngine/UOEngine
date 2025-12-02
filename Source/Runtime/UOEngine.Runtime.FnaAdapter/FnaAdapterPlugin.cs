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
                game.GraphicsDevice._renderContext = renderContext;
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
        _shaderRemapper.RemapEffect<SpriteEffect>(@"D:\UODev\UOEngineGitHub\Source\Shaders\FNACompat\SpriteEffect.hlsl", [new Technique
        {
            Name = "SpriteBatch",
            VertexMain = "SpriteVertexShader",
            PixelMain = "SpritePixelShader"
        }]);
    }

    public void RegisterGame(Game game)
    {
        _hostedFNAGames.Add(game);
    }
}
