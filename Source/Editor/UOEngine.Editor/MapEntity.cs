using UOEngine.Runtime.Core;
using UOEngine.Ultima.UOAssets;

namespace UOEngine.Editor;

internal class MapEntity: IEntity
{
    private Map _map = null!;

    private Chunk[] _chunks = [];

    private readonly EntityManager _entityManager;

    public MapEntity(EntityManager entityManager)
    {
        _entityManager = entityManager;
    }

    public void Load(Map map)
    {
        _map = map;

        _chunks = new Chunk[map.BlockSizeX *  map.BlockSizeY];
    }

    public Chunk? GetChunk(int chunkX, int chunkY)
    {
        if (chunkX < 0 || chunkY < 0)
        {
            return null;
        }

        int chunkIndex = GetChunkIndex(chunkX, chunkY);

        ref Chunk chunk = ref _chunks[chunkIndex];

        if(chunk != null)
        {
            return chunk;
        }

        chunk = _entityManager.NewEntity<Chunk>();

        chunk.Load(_map.BlockData[chunkIndex], chunkX, chunkY);

        return chunk;
    }

    public int GetChunkIndex(int chunkX, int chunkY)
    {
        if(chunkX < 0 || chunkY < 0)
        {
            return -1;
        }

        int index = chunkY + _map.BlockSizeY * chunkX;

        return index;
    }

    public void Update(float time)
    {
    }
}
