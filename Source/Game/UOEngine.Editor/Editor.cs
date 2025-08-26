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

[StructLayout(LayoutKind.Sequential)]
struct PerFrameData
{
    public Matrix4x4 Projection;
    public Matrix4x4 WorldToCamera;
}

public class Editor : IUOEngineApp
{
    readonly UOAssetLoader _assetLoader = UOAssetLoader.Instance;

    private Memory<Texture2D> _textures;

    private Memory<PerInstanceData> _perInstanceData;
    private RenderBuffer _renderBuffer;
    private uint _numTexturesProcessed = 0;

    private MapEntity _mapEntity;

    private CameraEntity _camera = new();

    private Window _window = new();

    private uint _numChunksX = 4;
    private uint _numChunksY = 1;

    public bool PreEngineInit()
    {
        _assetLoader.LoadAllFiles(@"D:\Program Files (x86)\Electronic Arts\Ultima Online Classic");

        return true;
    }

    public bool Initialise()
    {
        Console.WriteLine($"Game.Initialise: Start");

        //_camera.Transform.Position = new Vector3(-44.0f * 8 * 2, 44.0f * 8 * 2, 0.0f);

        _mapEntity = EntityManager.Instance.NewEntity<MapEntity>();

        _mapEntity.Load(_assetLoader.Maps[0]);

        uint maxTextures = 16000;

        uint maxInstances = 1024;

        _perInstanceData = new PerInstanceData[maxInstances];

        _textures = new Texture2D[maxTextures];

        _renderBuffer = new RenderBuffer(maxInstances, (uint)Unsafe.SizeOf<PerInstanceData>());

        ChunkTest();

        return true;
    }

    public void Update(float tick)
    {
        Vector2Int viewport = _window.Viewport;

        _camera.Projection = Matrix4x4.CreateOrthographic(0.0f, viewport.X, viewport.Y, 0.0f, -1.0f, 1.0f);
        _camera.Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, 0.5f * 0.5f * MathF.PI);

        ShaderInstance shaderInstance = RenderContext.GetShaderInstance();

        PerFrameData perFrameData = new PerFrameData();
        perFrameData.Projection = _camera.Projection;

        Vector3 offset = _camera.WorldToCameraMatrix.Position;

        Matrix4x4 m = Matrix4x4.Identity;

        m.SetColumn(3, offset);

        perFrameData.WorldToCamera = m;

        shaderInstance.SetVariable("cbPerFrameData", ref perFrameData);
        shaderInstance.SetBuffer("sbPerInstanceData", _renderBuffer);

        uint numTiles = _numChunksX * _numChunksY * 64;

        RenderContext.Draw(numTiles);
    }
    
    private void ChunkTest()
    {
        uint chunkStartX = 0;
        uint chunkStartY = 0;

        uint texelPerDim = 44;

        uint chunkSize = texelPerDim * 8;

        int num_chunks = 0;

        int graphicIdToTextureIndex = 0;

        int[] graphicIdToTextureIndices = new int[16000];

        Array.Fill(graphicIdToTextureIndices, -1);

        for (uint chunkY = 0; chunkY < _numChunksY; chunkY++)
        {
            for(uint chunkX = 0; chunkX < _numChunksX; chunkX++)
            {
                Chunk chunk = _mapEntity.GetChunk((int)chunkStartX + (int)chunkX, (int)chunkStartY + (int)chunkY);

                uint chunkIndex = chunkX + chunkY * _numChunksX;

                for (int y = 0; y < 8; y++)
                {
                    uint xPos = chunkX * chunkSize;
                    uint yPos = chunkY * chunkSize * (uint)y * 44;

                    for (int x = 0; x < 8; x++)
                    {
                        int index = (64 * (int)chunkIndex) + (x + y * 8);

                        int tileX = (int)chunkX * 8 + x;
                        int tileY = (int)chunkY * 8 + y;

                        Matrix4x4 modelToWorld = new Matrix4x4();

                        (int screenX, int screenY) = MapToScreen(tileX, tileY);

                        modelToWorld = Matrix4x4.Translate(new Vector3(screenX, screenY, 0.0f));

                        _perInstanceData.Span[index].ModelToWorld = modelToWorld;

                        var entity = chunk.Entities[x, y];

                        int textureIndex = graphicIdToTextureIndices[entity.GraphicId];

                        if (textureIndex == -1)
                        {
                            var bitmap = _assetLoader.GetLand(entity.GraphicId);

                            Texture2D texture = new(bitmap.Width, bitmap.Height, $"T_Land_{y}");

                            texture.SetPixels(bitmap.Texels);
                            texture.Apply();

                            textureIndex = graphicIdToTextureIndex;

                            _textures.Span[textureIndex] = texture;
                            graphicIdToTextureIndices[entity.GraphicId] = textureIndex;

                            graphicIdToTextureIndex++;

                        }

                        _perInstanceData.Span[index].TextureIndex = (uint)textureIndex;

                        xPos += 44;
                    }
                }
            }
        }

        _renderBuffer.SetData<PerInstanceData>(_perInstanceData.Span);
        RenderContext.SetBindlessTextures(_textures.Span, (uint)graphicIdToTextureIndex);
    }

    (int screenX, int screenY) MapToScreen(int x, int y)
    {
        int TileWidth = 44;
        int TileHeight = 44;

        int screenX = (x - y) * (TileWidth / 2);
        int screenY = (x + y) * (TileHeight / 2);
        return (screenX, screenY);
    }
}
