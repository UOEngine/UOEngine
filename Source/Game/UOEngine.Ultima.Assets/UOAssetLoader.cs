using UOEngine.PackageFile;

namespace UOEngine.UOAssets;



public class UOAssetLoader
{
    public UOPackageFile Gumps { get; private set; }

    public const int NumMaps = 6;
    public UOMapFileData[] MapFileDatas = new UOMapFileData[NumMaps];

    public void LoadAllFiles(string ultimaOnlineDirectory)
    {
        LoadGumps(ultimaOnlineDirectory);

        MountMaps(ultimaOnlineDirectory);
        //MapAssets.Load(ultimaOnlineDirectory);
    }

    public UOBitmap GetGump(int idx)
    {
        var bitmap = new UOBitmap();

        bitmap.DeserialiseFromUOPackageFileResource(Gumps.GetResource(idx));

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
        int mapCount = 6;

        var mapsDefaultSize = new int[6, 2]
        {
            {7168, 4096},
            {7168, 4096},
            {2304, 1600},
            {2560, 2048},
            {1448, 1448},
            {1280, 4096}
        };

        var maps = new Map[mapCount];

        for (int i = 0; i < mapCount; i++)
        {
            MapFileDatas[i] = new UOMapFileData(ultimaOnlineDirectory, i);

            MapFileDatas[i].Mount();

            maps[i] = new Map();

            maps[i].Deserialise(mapsDefaultSize[i, 0], mapsDefaultSize[i, 1], MapFileDatas[0].MapLegacyMul, true, MapFileDatas[0].IdxStatics.Reader);
        }
    }
}
