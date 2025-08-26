using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOEngine.PackageFile;
using UOEngine.UOAssets;

namespace UOEngine;

internal class Chunk
{
    public readonly LandEntity[,] Entities = new LandEntity[8,8];

    public void Load(IndexMap indexMap, int blockX, int blockY)
    {
        indexMap.MapFile.Reader.BaseStream.Seek((long)indexMap.MapAddress, SeekOrigin.Begin);

        var mapBlock = indexMap.MapFile.Reader.Read<MapBlock>();

        int cornerX = blockX << 3;
        int cornerY = blockY << 3;

        for(int y = 0; y < 8; y++)
        {
            int pos = y << 3;
            ushort tileY = (ushort)(cornerY + y);

            for (int x = 0; x < 8; x++, ++pos)
            {
                ushort tileID = (ushort)(mapBlock.Cells[pos].TileID & 0x3FFF);
                ushort tileX = (ushort)(cornerX + x);

                LandEntity land = EntityManager.Instance.NewEntity(() => new LandEntity(tileID));

                int tileIndex = tileY + 8 * tileX;

                Entities[x, y] = land;
            }
        }
    }
}
