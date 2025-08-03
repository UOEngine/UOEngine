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

    private Memory<Texture2D> _textures;

    private Memory<PerInstanceData> _modelToWorld;
    private RenderBuffer _renderBuffer;
    private uint _numTexturesProcessed = 0;

    public bool PreEngineInit()
    {
        _assetLoader.LoadAllFiles(@"D:\Program Files (x86)\Electronic Arts\Ultima Online Classic");

        return true;
    }

    public bool Initialise()
    {
        Debug.WriteLine($"Game.Initialise: Start");

        var mapEntity = EntityManager.Instance.NewEntity<MapEntity>();

        mapEntity.Load(_assetLoader.Maps[0]);

        uint screen_height = 2160;
        uint screen_width = 3840;

        uint texelPerDim = 44;

        uint num_textures_width = screen_width / texelPerDim;
        uint num_textures_height = screen_height / texelPerDim;

        uint numTextures = num_textures_width * num_textures_height;

        uint max_index = 16000;

        uint max = max_index;

        _modelToWorld = new PerInstanceData[max];

        _textures = new Texture2D[max];

        uint index = 0;

        //uint xPos = 0;

        int num_textures_processed = 0;

        for (int y = 0; y < num_textures_height; y++)
        {
            uint xPos = 0;
            uint yPos = (uint)y * texelPerDim;

            for (int x = 0; ; x++)
            {
                index = (uint)x + (uint)y * num_textures_width;

                if (index >= max_index)
                {
                    break;
                }

                var bitmap = _assetLoader.GetLand((uint)index);

                if(bitmap.IsEmpty)
                {
                    continue;
                }

                Texture2D texture = new(bitmap.Width, bitmap.Height, $"T_Land_{index}");

                texture.SetPixels(bitmap.Texels);
                texture.Apply();

                _textures.Span[num_textures_processed] = texture;

                Matrix4x4 modelToWorld = new Matrix4x4();

                modelToWorld = Matrix4x4.Translate(new Vector3(xPos, yPos, 0.0f));

                _modelToWorld.Span[num_textures_processed].ModelToWorld = modelToWorld;
                _modelToWorld.Span[num_textures_processed].TextureIndex = (uint)num_textures_processed;

                num_textures_processed++;

                xPos += texelPerDim;

                if (xPos > screen_width)
                {
                    //xPos = 0;
                    break;
                }
            }

            if (index > max_index)
            {
                break;
            }
        }

        _renderBuffer = new RenderBuffer((uint)num_textures_processed, (uint)Unsafe.SizeOf<PerInstanceData>());

        _renderBuffer.SetData<PerInstanceData>(_modelToWorld.Span.Slice(0, (int)num_textures_processed));

        RenderContext.SetBindlessTextures(_textures.Span.Slice(0, num_textures_processed));

        return true;
    }

    public void Update(float tick)
    {
        ShaderInstance shaderInstance = RenderContext.GetShaderInstance();

        shaderInstance.SetBuffer("sbPerInstanceData", _renderBuffer);

        RenderContext.Draw((uint)_textures.Length);
    }
}
