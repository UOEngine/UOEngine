using UOEngine.Runtime.Core;
using UOEngine.UltimaOnline.Assets.Maps;

namespace UOEngine.UltimaOnline.Game
{
    public class Chunk
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public Chunk(int x, int y)
        {
            X = x;
            Y = y;
        }
        public void Load(IndexMap chunkData)
        {
            if(chunkData.MapAddress == 0)
            {
                UOE.Assert(false);
            }
        }
    }

    public class UOMap
    {
        private Chunk[] _chunks = [];
        private int     _width = 0;
        private int     _height = 0;

        public void Load(IndexMap[] mapData, int mapWidth, int mapHeight)
        {
            int numChunks = mapWidth * mapHeight;

            _chunks = new Chunk[numChunks];
            _width = mapWidth;
            _height = mapHeight;

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    int index = GetBlockIndex(x, y);
                    var chunk = new Chunk(x, y);

                    chunk.Load(mapData[index]);

                    _chunks[index] = chunk;
                }
            }
        }

        public Chunk GetChunk(int tileX, int tileY)
        {
            int chunkX = tileX >> 3;
            int chunkY = tileY >> 3;
            int block = GetBlockIndex(chunkX, chunkY);

            return _chunks[block];
        }

        private int GetBlockIndex(int blockX, int blockY)
        {
            return blockX * _height + blockY;
        }
    }
}
