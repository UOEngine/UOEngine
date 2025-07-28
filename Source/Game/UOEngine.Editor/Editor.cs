using System.Diagnostics;
using System.Runtime.CompilerServices;
using UOEngine.Core;
using UOEngine.UOAssets;

namespace UOEngine.Editor;

public class Editor : IUOEngineApp
{
    readonly UOAssetLoader _assetLoader = new();

    private Texture2D? _texture;

    private List<Texture2D> _textures = [];
    private Memory<Matrix4x4> _modelToWorld;
    private RenderBuffer _renderBuffer; 

    public bool PreEngineInit()
    {
        _assetLoader.LoadAllFiles(@"D:\Program Files (x86)\Electronic Arts\Ultima Online Classic");

        return true;
    }

    public bool Initialise()
    {
        Debug.WriteLine($"Game.Initialise: Start");

        //var loginBackgroundBitmap = _assetLoader.GetGump((int)EGumpTypes.LoginBackground);

        //_texture = new(loginBackgroundBitmap.Width, loginBackgroundBitmap.Height);

        //_texture.SetPixels(loginBackgroundBitmap.Texels);
        //_texture.Apply();

        var mapEntity = EntityManager.Instance.NewEntity<MapEntity>();

        mapEntity.Load(_assetLoader.Maps[0]);

        Chunk chunk = mapEntity.GetChunk(0, 0);

        uint numTextures = 2;

        _modelToWorld = new Matrix4x4[numTextures];

        _renderBuffer = new RenderBuffer(numTextures, (uint)Unsafe.SizeOf<Matrix4x4>());

        for (int i = 0; i < numTextures; i++)
        {
            var bitmap = _assetLoader.GetLand((uint)i);

            _texture = new(bitmap.Width, bitmap.Height);
            _texture.SetPixels(bitmap.Texels);
            _texture.Apply();

            _textures.Add(_texture);

            Matrix4x4 modelToWorld = new Matrix4x4();

            modelToWorld = Matrix4x4.Translate(new Vector3(44.0f * i, 0.0f, 0.0f));

            _modelToWorld.Span[i] = modelToWorld;
        }

        _renderBuffer.SetData<Matrix4x4>(_modelToWorld.Span);

        return true;
    }

    public void Update(float tick)
    {
        ShaderInstance shaderInstance = RenderContext.GetShaderInstance();

        for (int i = 0; i < _textures.Count; i++)
        {
            shaderInstance.SetTexture("texture", _textures[i]);
            shaderInstance.SetBuffer("sbPerInstanceData", _renderBuffer);
            shaderInstance.SetMatrix("modelToWorld", _modelToWorld.Span[i]);

            RenderContext.Draw();
        }
    }
}
