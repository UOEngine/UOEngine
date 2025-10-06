﻿using UOEngine.Runtime;
using UOEngine.Ultima.UOAssets;

namespace UOEngine.Editor;

internal class LandEntity: IEntity
{
    public readonly ushort GraphicId;
    public readonly sbyte AverageZ;
    public readonly bool IsStretched;
    public readonly sbyte MinZ;
    public readonly bool CanDraw;

    public LandEntity(ushort graphicId, UOAssetLoader assetLoader)
    {
        var tileData = assetLoader.LandTiles[graphicId];

        GraphicId = graphicId;
        IsStretched = tileData.TexID == 0 && tileData.IsWet;
        CanDraw = graphicId > 2;

        //if (TextureCache.Instance.Contains(graphicId) == false)
        //{
        //    var bitmap = UOAssetLoader.Instance.GetLand(GraphicId);

        //    Texture2D texture = new(bitmap.Width, bitmap.Height, $"T_Land_{GraphicId}");

        //    texture.SetPixels(bitmap.Texels);
        //    texture.Apply();

        //    TextureCache.Instance.AddTexture(graphicId, texture);
        //}
    }

    public void Update(TimeSpan time)
    {
    }
}
