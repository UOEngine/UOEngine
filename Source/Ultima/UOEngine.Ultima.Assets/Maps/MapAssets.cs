using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using UOEngine.Ultima.PackageFile;

namespace UOEngine.Ultima.UOAssets;

public readonly struct UOMapFileData
{
    public readonly UOPackageFile MapLegacyMul;
    public readonly UOPackageFile MapXLegacyMul;
    public readonly UOPackageFile Statics;
    public readonly UOPackageFile IdxStatics;
    public readonly UOPackageFile StaticsX;
    public readonly UOPackageFile IdxStaticsX;
    public readonly int Index;

    public UOMapFileData(string ultimaOnlineDirectory, int mapIndex)
    {
        Index = mapIndex;

        string packedMapFilePattern = $"build/map{mapIndex}legacymul/{{0:D8}}.dat";
        string path = Path.Combine(ultimaOnlineDirectory, $"map{mapIndex}LegacyMUL.uop");

        MapLegacyMul = new UOPackageFile(path);

        path = Path.Combine(ultimaOnlineDirectory, $"map{mapIndex}xLegacyMUL.uop");

        MapXLegacyMul = new UOPackageFile(path);

        path = Path.Combine(ultimaOnlineDirectory, $"statics{mapIndex}.mul");

        Statics = new UOPackageFile(path);

        path = Path.Combine(ultimaOnlineDirectory, $"staidx{mapIndex}.mul");

        IdxStatics = new UOPackageFile(path);

        path = Path.Combine(ultimaOnlineDirectory, $"statics{mapIndex}x.mul");

        StaticsX = new UOPackageFile(path);

        path = Path.Combine(ultimaOnlineDirectory, $"staidx{mapIndex}x.mul");

        IdxStaticsX = new UOPackageFile(path);
    }

    public void Mount()
    {
        MapLegacyMul.Load($"build/map{Index}legacymul/{{0:D8}}.dat", false);

        if (MapXLegacyMul.Exists)
        {
            MapXLegacyMul.Load($"build/map{Index}legacymul/{{0:D8}}.dat", false);
        }

        if (Statics.Exists)
        {
            Statics.Mount();
        }

        if (IdxStatics.Exists)
        {
            IdxStatics.Mount();
        }

        if (StaticsX.Exists)
        {
            StaticsX.Mount();
        }

        if (IdxStaticsX.Exists)
        {
            IdxStaticsX.Mount();
        }
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct StaticsBlock
{
    public ushort Color;
    public byte X;
    public byte Y;
    public sbyte Z;
    public ushort Hue;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct StaidxBlock
{
    public uint Position;
    public uint Size;
    public uint Unknown;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public ref struct StaidxBlockVerdata
{
    public uint   Position;
    public ushort Size;
    public byte   Unknown;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MapCells
{
    public ushort TileID;
    public sbyte Z;
}

[InlineArray(64)]
public struct MapCellsArray
{
    private MapCells _a0;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MapBlock
{
    public uint Header;
    public unsafe MapCellsArray Cells;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RadarMapcells
{
    public ushort Graphic;
    public sbyte Z;
    public bool IsLand;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RadarMapBlock
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public RadarMapcells[,] Cells;
}

public struct IndexMap
{
    public UOPackageFile MapFile;
    public UOPackageFile StaticFile;
    public ulong MapAddress;
    public ulong OriginalMapAddress;
    public ulong OriginalStaticAddress;
    public uint OriginalStaticCount;
    public ulong StaticAddress;
    public uint StaticCount;
    public static IndexMap Invalid = new IndexMap();
}
