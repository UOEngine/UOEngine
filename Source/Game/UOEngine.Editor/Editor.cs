using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using UOEngine.Core;
using UOEngine.UOAssets;
using UOEngine.Renderer;

namespace UOEngine.Editor;

[StructLayout(LayoutKind.Sequential)]
struct PerInstanceData
{
    public Vector3 TileCentre;
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

    private Memory<PerInstanceData> _perInstanceData;
    private RenderBuffer _renderBuffer;

    private MapEntity _mapEntity;

    private CameraEntity _camera = new();

    private Window _window = new();

    public bool PreEngineInit()
    {
        _assetLoader.LoadAllFiles(@"D:\Program Files (x86)\Electronic Arts\Ultima Online Classic");

        return true;
    }

    public bool Initialise()
    {
        Console.WriteLine($"Game.Initialise: Start");

        _camera.Transform.Position = new Vector3(0, 0, 1.0f);

        var rotationAroundRight = new Quaternion(_camera.Transform.Rotation.RightVector, 0.5f * MathF.PI);

        Vector3 newCamForward = rotationAroundRight.Rotate(_camera.Transform.Rotation.ForwardVector);

        var rotationAroundForward = new Quaternion(newCamForward, 0.5f * MathF.PI);

        _camera.Transform.Rotation = rotationAroundForward * rotationAroundRight;

        _camera.Transform.Rotation = new Quaternion(0.5f, 0.5f, -0.5f, 0.5f);


        _mapEntity = EntityManager.Instance.NewEntity<MapEntity>();

        _mapEntity.Load(_assetLoader.Maps[0]);

        uint maxInstances = 2 * 4096;

        _perInstanceData = new PerInstanceData[maxInstances];

        _renderBuffer = new RenderBuffer(maxInstances, (uint)Unsafe.SizeOf<PerInstanceData>());

        return true;
    }

    public void Update(float tick)
    {
        Vector2Int viewport = _window.Viewport;

        _camera.Projection = Matrix4x4.CreateOrthographic(0, viewport.X, viewport.Y, 0.0f, -1.0f, 1.0f);
        //_camera.Projection = Matrix4x4.CreateOrthographic(viewport.X, viewport.Y, 1.0f, 1.0f);

        //_camera.Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Right, -0.5f * MathF.PI);


        //Rotator rotation2 = _camera.Transform.Rotation.ToEulerAngles().ToDegrees();

        Matrix4x4 renderMatrix = new Matrix4x4(new Vector4(0, 1, 0, 0),
                                               new Vector4(0, 0, 1, 0),
                                               new Vector4(1, 0, 0, 0),
                                               new Vector4(0, 0, 0, 1));


        ShaderInstance shaderInstance = RenderContext.GetShaderInstance();

        PerFrameData perFrameData = new PerFrameData();

        perFrameData.Projection = _camera.Projection;
        //perFrameData.WorldToCamera = _camera.WorldToCameraMatrix;

        var camMatrix = _camera.WorldToCameraMatrix;

        perFrameData.WorldToCamera = renderMatrix * camMatrix;

        int numInstances = UpdateVisibleChunks();

        shaderInstance.SetVariable("cbPerFrameData", perFrameData);
        shaderInstance.SetBuffer("sbPerInstanceData", _renderBuffer);

        RenderContext.Draw((uint)numInstances);
    }

    private int UpdateVisibleChunks()
    {
        Vector2Int viewport = _window.Viewport;

        Vector3 cameraPosition = _camera.Transform.Position;

        int texelPerDim = (int)Math.Sqrt(2 *22 * 22);
        int halfTexelsPerDim = (int)(0.5f * texelPerDim);

        int chunkSize = 44 * 8;

        int numChunksX = 10;// (int)MathF.Ceiling(viewport.X / chunkSize);
        int numChunksY = 10;// (int)MathF.Ceiling(viewport.Y / chunkSize);

        int halfChunkX = numChunksX / 2;
        int halfChunkY = numChunksY / 2;

        int cameraTileX = (int)cameraPosition.X / texelPerDim;
        int cameraTileY = (int)cameraPosition.Y / texelPerDim;

        int cameraChunkX = cameraTileX  >> 3;
        int cameraChunkY = cameraTileY >> 3;

        int instanceIndex = 0;

        for (int chunkX = (cameraChunkX - halfChunkX); chunkX <= (cameraChunkX + halfChunkX); chunkX++)
        {
            for (int chunkY = (cameraChunkY - halfChunkY); chunkY <= (cameraChunkY + halfChunkY); chunkY++)
            {
                Chunk? chunk = _mapEntity.GetChunk(chunkX, chunkY);

                if (chunk == null)
                {
                    continue;
                }

                int chunkIndex = _mapEntity.GetChunkIndex(chunkX, chunkY);

                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        int tileX = chunkX * 8 + x;
                        int tileY = chunkY * 8 + y;

                        //Matrix4x4 modelToWorld = Matrix4x4.Translate(new Vector3(tileX * texelPerDim, tileY * texelPerDim, 0.0f));

                        float tileCentreX = tileX * texelPerDim;
                        float tileCentreY = tileY * texelPerDim;

                        Vector3 tileCentre = new Vector3( tileCentreX, tileCentreY, 0.0f);

                        ref var entity = ref chunk.Entities[x, y];
                        ref var instanceData = ref _perInstanceData.Span[instanceIndex];

                        instanceData.TileCentre = tileCentre;
                        instanceData.TextureIndex = (uint)TextureCache.Instance.GetRenderResourceId(entity.GraphicId);

                        instanceIndex++;
                    }

                }
            }
        }

        _renderBuffer.SetData<PerInstanceData>(_perInstanceData.Span);

        return instanceIndex;
    }
}
