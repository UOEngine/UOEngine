using Microsoft.Extensions.DependencyInjection;
using UOEngine.Plugin;
using UOEngine.Runtime;
using UOEngine.Ultima.UOAssets;

namespace UOEngine.Editor;

internal class EditorPlugin : IPlugin
{
    private EntityManager _entityManager = null!;
    private CameraEntity? _camera;
    private readonly UOAssetLoader _assetLoader;
    private MapEntity _map = null!;

    public EditorPlugin(IServiceProvider serviceProvider) 
    {
        _entityManager = serviceProvider.GetRequiredService<EntityManager>();
        _assetLoader = serviceProvider.GetRequiredService<UOAssetLoader>();

        serviceProvider.GetRequiredService<ApplicationLoop>().OnUpdate += Update;
    }

    public void Startup()
    {
        _assetLoader.LoadAllFiles("D:\\Program Files (x86)\\Electronic Arts\\Ultima Online Classic");

        _camera = _entityManager.NewEntity<CameraEntity>();
        _map = _entityManager.NewEntity<MapEntity>();

        _map.Load(_assetLoader.Maps[0]);

        var water = _map.GetChunk(0, 0).Entities[0, 0];

        var bitmap = _assetLoader.GetLand(water.GraphicId);
    }

    public void Update(TimeSpan time)
    {

    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<UOAssetLoader>();
    }
}
