using Microsoft.Extensions.DependencyInjection;
using UOEngine.Plugin;
using UOEngine.Runtime;
using UOEngine.Runtime.Renderer;
using UOEngine.Ultima.UOAssets;

namespace UOEngine.Editor;

internal class EditorPlugin : IPlugin
{
    private readonly EntityManager _entityManager;
    private readonly RenderFactory _renderFactory;
    private readonly Renderer _renderer;

    private CameraEntity? _camera;
    private readonly UOAssetLoader _assetLoader;
    private MapEntity _map = null!;
    private UOETexture _testTexture = null!;

    public EditorPlugin(IServiceProvider serviceProvider) 
    {
        _entityManager = serviceProvider.GetRequiredService<EntityManager>();
        _assetLoader = serviceProvider.GetRequiredService<UOAssetLoader>();
        _renderFactory = serviceProvider.GetRequiredService<RenderFactory>();
        _renderer = serviceProvider.GetRequiredService<Renderer>();

        _renderer.OnFrameBegin += OnFrameBegin;

        //serviceProvider.GetRequiredService<ApplicationLoop>().OnUpdate += Update;
    }

    public void Startup()
    {
        _assetLoader.LoadAllFiles("D:\\Program Files (x86)\\Electronic Arts\\Ultima Online Classic");

        _camera = _entityManager.NewEntity<CameraEntity>();
        _map = _entityManager.NewEntity<MapEntity>();

        _map.Load(_assetLoader.Maps[0]);

        var water = _map.GetChunk(0, 0).Entities[0, 0];

        var bitmap = _assetLoader.GetLand(water.GraphicId);

        _testTexture = _renderFactory.CreateTexture((int)bitmap.Width, (int)bitmap.Height);

        _testTexture.SetTexels(bitmap.Texels.ToArray());

    }

    public void OnFrameBegin(RenderContext renderContext)
    {
        renderContext.SetTexture(_testTexture);
        renderContext.Draw();
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<UOAssetLoader>();
    }
}
