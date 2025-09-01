using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using UOEngine.UOAssets;

namespace UOEngine;

internal class MapEntity: IEntity
{
    private Map _map;

    private Chunk[] _chunks = [];

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

        chunk = new Chunk();

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
}
