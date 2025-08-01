﻿using UOEngine.PackageFile;

namespace UOEngine.UOAssets;

public class UOAssetLoader
{
    public UOPackageFile Gumps { get; private set; }

    public const int NumMaps = 6;
    public UOMapFileData[] MapFileDatas = new UOMapFileData[NumMaps];

    public Map[] Maps = new Map[NumMaps];

    public UOPackageFile Art { get; private set; }

    public void LoadAllFiles(string ultimaOnlineDirectory)
    {
        LoadGumps(ultimaOnlineDirectory);

        MountMaps(ultimaOnlineDirectory);
        //MapAssets.Load(ultimaOnlineDirectory);

        MountArt(ultimaOnlineDirectory);
    }

    public UOBitmap GetGump(int idx)
    {
        var bitmap = new UOBitmap();

        bitmap.DeserialiseFromUOPackageFileResource(Gumps.GetResource(idx));

        return bitmap;
    }

    public UOBitmap GetLand(uint idx)
    {
        return GetArtInternal((uint)(idx & ~0x4000));
    }

    private UOBitmap GetArtInternal(uint idx)
    {
        var bitmap = new UOBitmap();

        UOFileIndex entry = Art.FileIndices[(int)idx];

        Art.Reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

        bitmap.Width = 44;
        bitmap.Height = 44;

       bitmap.Texels = new uint[bitmap.Width * bitmap.Height];

        for (int i = 0; i < 22; ++i)
        {
            int start = 22 - (i + 1);
            int pos = i * 44 + start;
            int end = start + ((i + 1) << 1);

            for (int j = start; j < end; ++j)
            {
                bitmap.Texels[pos++] = HuesHelper.Color16To32(Art.Reader.ReadUInt16()) | 0xFF_00_00_00;
            }
        }

        for (int i = 0; i < 22; ++i)
        {
            int pos = (i + 22) * 44 + i;
            int end = i + ((22 - i) << 1);

            for (int j = i; j < end; ++j)
            {
                bitmap.Texels[pos++] = HuesHelper.Color16To32(Art.Reader.ReadUInt16()) | 0xFF_00_00_00;
            }
        }

        return bitmap;
    }

    private void LoadGumps(string ultimaOnlineDirectory)
    {
        var gumpAssets = Path.Combine(ultimaOnlineDirectory, "gumpartLegacyMUL.uop");

        Gumps = new UOPackageFile(gumpAssets);

        Gumps.Load("build/gumpartlegacymul/{0:D8}.tga", true);
        // gump.def?
    }

    private void MountMaps(string ultimaOnlineDirectory)
    {
        var mapsDefaultSize = new int[NumMaps, 2]
        {
            {7168, 4096},
            {7168, 4096},
            {2304, 1600},
            {2560, 2048},
            {1448, 1448},
            {1280, 4096}
        };

        for (int i = 0; i < NumMaps; i++)
        {
            MapFileDatas[i] = new UOMapFileData(ultimaOnlineDirectory, i);

            MapFileDatas[i].Mount();

            Maps[i] = new Map();

            Maps[i].Deserialise(mapsDefaultSize[i, 0], mapsDefaultSize[i, 1], MapFileDatas[i], true);
        }
    }

    private void MountArt(string ultimaOnlineDirectory)
    {
        var artAssets = Path.Combine(ultimaOnlineDirectory, "artLegacyMUL.uop");

        Art = new UOPackageFile(artAssets);

        Art.Load("build/artlegacymul/{0:D8}.tga", false);
    }
}
