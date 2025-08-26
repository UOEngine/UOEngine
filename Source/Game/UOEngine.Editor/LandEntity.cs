using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOEngine.UOAssets;

namespace UOEngine;

internal class LandEntity: IEntity
{
    public readonly ushort GraphicId;
    public readonly sbyte AverageZ;
    public readonly bool IsStretched;
    public readonly sbyte MinZ;
    public readonly bool CanDraw;

    public LandEntity(ushort graphicId)
    {
        var tileData = UOAssetLoader.Instance.LandTiles[graphicId];

        GraphicId = graphicId;
        IsStretched = tileData.TexID == 0 && tileData.IsWet;
        CanDraw = graphicId > 2;
    }
}
