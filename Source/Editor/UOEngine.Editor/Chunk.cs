// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Core;
using UOEngine.Ultima.UOAssets;
using UOEngine.Ultima.PackageFile;

namespace UOEngine.Editor;

internal class Chunk: IEntity
{
    public readonly LandEntity[,] Entities = new LandEntity[8,8];

    private readonly EntityManager _entityManager;
    private readonly UOAssetLoader _assetLoader;
    public Chunk(EntityManager entityManager, UOAssetLoader assetLoader)
    {
        _entityManager = entityManager;
        _assetLoader = assetLoader;
    }

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

                LandEntity land = _entityManager.NewEntity<LandEntity>(tileID, _assetLoader);

                int tileIndex = tileY + 8 * tileX;

                Entities[x, y] = land;
            }
        }
    }

    public void Update(float time)
    {
    }
}
