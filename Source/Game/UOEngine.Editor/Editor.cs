using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UOEngine.Core;
using UOEngine.UOAssets;

namespace UOEngine.Editor;

[StructLayout(LayoutKind.Sequential)]
struct PerInstanceData
{
    public Matrix4x4 ModelToWorld;
    public uint TextureIndex;
}

public class Editor : IUOEngineApp
{
    readonly UOAssetLoader _assetLoader = new();

    private Texture2D[] _textures = [];

    private Memory<PerInstanceData> _modelToWorld;
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

        uint numTextures = 8;

        uint texelPerDim = 44;

        _modelToWorld = new PerInstanceData[numTextures];

        _textures = new Texture2D[numTextures];

        _renderBuffer = new RenderBuffer((uint)numTextures, (uint)Unsafe.SizeOf<PerInstanceData>());

        for (int i = 0; i < numTextures; i++)
        {
            var bitmap = _assetLoader.GetLand((uint)i);

            Texture2D texture = new(bitmap.Width, bitmap.Height);

            texture.SetPixels(bitmap.Texels);
            texture.Apply();

            _textures[i] = texture;

            Matrix4x4 modelToWorld = new Matrix4x4();

            modelToWorld = Matrix4x4.Translate(new Vector3(44.0f * i, 0.0f, 0.0f));

            _modelToWorld.Span[i].ModelToWorld = modelToWorld;
            _modelToWorld.Span[i].TextureIndex = (uint)i;
        }

        _renderBuffer.SetData<PerInstanceData>(_modelToWorld.Span);

        RenderContext.SetBindlessTextures(_textures);

        return true;
    }

    public void Update(float tick)
    {
        ShaderInstance shaderInstance = RenderContext.GetShaderInstance();

        shaderInstance.SetBuffer("sbPerInstanceData", _renderBuffer);

        RenderContext.Draw((uint)_textures.Length);
    }
}
