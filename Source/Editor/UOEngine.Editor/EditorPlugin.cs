//using System.Numerics;
//using Microsoft.Extensions.DependencyInjection;
//using UOEngine.Runtime.Core;
//using UOEngine.Runtime.Plugin;
//using UOEngine.Runtime.Renderer;
//using UOEngine.Runtime.RHI;
//using UOEngine.Runtime.RHI.Resources;
//using UOEngine.Ultima.UOAssets;

//namespace UOEngine.Editor;

//internal class EditorPlugin : IPlugin
//{
//    private readonly EntityManager _entityManager;
//    private readonly IRenderResourceFactory _renderFactory;
//    private readonly RenderSystem _rendererSystem;

//    private CameraEntity? _camera;
//    private readonly UOAssetLoader _assetLoader;
//    private MapEntity _map = null!;
//    private UOETexture _testTexture = null!;
//    private ShaderInstance _shaderInstance;
//    public EditorPlugin(IServiceProvider serviceProvider)
//    {
//        _entityManager = serviceProvider.GetRequiredService<EntityManager>();
//        _assetLoader = serviceProvider.GetRequiredService<UOAssetLoader>();
//        _renderFactory = serviceProvider.GetRequiredService<IRenderResourceFactory>();
//        _rendererSystem = serviceProvider.GetRequiredService<RenderSystem>();

//        _rendererSystem.OnFrameBegin += OnFrameBegin;

//        //_renderFactory.CreateShaderInstance(null);

//        //serviceProvider.GetRequiredService<ApplicationLoop>().OnUpdate += Update;
//    }

//    public void Startup()
//    {
//        //_assetLoader.LoadAllFiles("D:\\Program Files (x86)\\Electronic Arts\\Ultima Online Classic");

//        //_camera = _entityManager.NewEntity<CameraEntity>();
//        //_map = _entityManager.NewEntity<MapEntity>();

//        //_map.Load(_assetLoader.Maps[0]);

//        //var water = _map.GetChunk(0, 0).Entities[0, 0];

//        //var bitmap = _assetLoader.GetLand(water.GraphicId);

//        //_testTexture = _renderFactory.CreateTexture((int)bitmap.Width, (int)bitmap.Height);

//        //_testTexture.SetTexels(bitmap.Texels.ToArray());

//    }

//    public void OnFrameBegin(IRenderContext renderContext)
//    {
//        Matrix4x4 projection = Matrix4x4.Identity;

//        var mvp = new ModelViewProjection
//        {
//            Projection = Matrix4x4.Identity,
//            View = Matrix4x4.CreateTranslation(-0.5f, -0.5f, 0.0f)
//        };

//        _shaderInstance.SetData(_projectionBinding, mvp);
//        _shaderInstance.SetTexture(_textureBindingHandle, _checkerboardTexture);
//        _shaderInstance.SetSampler(_samplerBindingHandle, new RhiSampler { Filter = SamplerFilter.Point });

//        context.GraphicsPipline = _pipeline;
//        context.ShaderInstance = _shaderInstance;

//        context.DrawIndexedPrimitives(1);
//    }

//    public static void ConfigureServices(IServiceCollection services)
//    {
//        services.AddSingleton<UOAssetLoader>();
//    }
//}
