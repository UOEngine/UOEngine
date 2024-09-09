using UOEngine.Runtime.Core;
using UOEngine.UltimaOnline.Assets;
using UOEngine.UltimaOnline.Assets.Maps;

namespace UOEngine.UltimaOnline.Game
{
    public readonly struct UOTile(ushort id, sbyte height)
    {
        public readonly ushort  Id = id;
        public readonly sbyte   Height = height;
    }

    public class UOChunk
    {
        public int          X { get; private set; }
        public int          Y { get; private set; }

        public UOTile[,] Tiles { get; } = new UOTile[8, 8];

        public UOChunk(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void Load(IndexMap chunkData)
        {
            if(chunkData.MapAddress != 0)
            {
                LoadLand(chunkData);

                return;
            }

            if(chunkData.StaticAddress != 0)
            {
                LoadStatic(chunkData);
            }
        }

        private void LoadLand(IndexMap chunkData)
        {
            chunkData.MapFile.Stream!.Seek((long)chunkData.MapAddress, SeekOrigin.Begin);

            var block = chunkData.MapFile.Reader!.Read<MapBlock>();

            var cells = block.Cells;
            int bx = X << 3;
            int by = Y << 3;

            for (int y = 0; y < 8; ++y)
            {
                int pos = y << 3;
                ushort tileY = (ushort)(by + y);

                for (int x = 0; x < 8; ++x, ++pos)
                {
                    ushort tileID = (ushort)(cells[pos].TileID & 0x3FFF);

                    sbyte height = cells[pos].Z;

                    ushort tileX = (ushort)(bx + x);

                    Tiles[x, y] = new UOTile(tileID, height);
                }
            }
        }

        private void LoadStatic(IndexMap chunkData)
        {

        }
    }

    public class UOMap
    {
        private UOChunk[] _chunks = [];
        private int     _width = 0;
        private int     _height = 0;

        public void Load(IndexMap[] mapData, int mapWidth, int mapHeight)
        {
            int numChunks = mapWidth * mapHeight;

            _chunks = new UOChunk[numChunks];
            _width = mapWidth;
            _height = mapHeight;

            //Test(mapData, 659, 145);

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    int index = GetBlockIndex(x, y);
                    var chunk = new UOChunk(x, y);

                    chunk.Load(mapData[index]);

                    _chunks[index] = chunk;
                }
            }
        }

        public UOChunk GetChunk(int tileX, int tileY)
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

        private void Test(IndexMap[] mapData, int x, int y)
        {
            int index = GetBlockIndex(x, y);
            var chunk = new UOChunk(x, y);

            chunk.Load(mapData[index]);

            _chunks[index] = chunk;
        }
    }
}
