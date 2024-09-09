using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.UltimaOnline.Assets.Maps
{
    //public readonly struct UOMapFileData
    //{
    //    public readonly UOFile MapLegacyMul;
    //    public readonly UOFile MapXLegacyMul;
    //    public readonly UOFile Statics;
    //    public readonly UOFile IdxStatics;
    //    public readonly UOFile StaticsX;
    //    public readonly UOFile IdxStaticsX;

    //    public UOMapFileData(string uoPath, int mapIndex)
    //    {
    //        MapLegacyMul = new UOFile();
    //        MapLegacyMul.Load(path, $"build/map{mapIndex}legacymul/{{0:D8}}.dat", false);


    //    }
    //}

    public class UOMapAssets
    {
        public const int            NumMaps = 6;

        public readonly UOFile[]    MapLegacyMUL = new UOFile[NumMaps];
        public readonly UOFile[]    MapXLegacyMUL = new UOFile[NumMaps];
        public readonly UOFile[]    Statics = new UOFile[NumMaps];
        public readonly UOFile[]    StaticsIdx = new UOFile[NumMaps];
        public readonly UOFile[]    StaticsX = new UOFile[NumMaps];
        public readonly UOFile[]    StaticsXIdx = new UOFile[NumMaps];

        public IndexMap[][]         BlockData { get; private set; } = new IndexMap[NumMaps][];

        public int[,]               MapsDefaultSize { get; protected set; }

        public int[,]               MapBlocksSize { get; private set; } = new int[NumMaps, 2];

        public UOMapAssets()
        {
            MapsDefaultSize = new int[6, 2]
            {
                {7168, 4096},
                {7168, 4096},
                {2304, 1600},
                {2560, 2048},
                {1448, 1448},
                {1280, 4096}
            };
        }

        public void Load(string ultimaOnlineDirectory)
        {
            for (int i = 0; i < NumMaps; i++)
            {
                string packedMapFilePattern = $"build/map{i}legacymul/{{0:D8}}.dat";
                string path = Path.Combine(ultimaOnlineDirectory, $"map{i}LegacyMUL.uop");

                MapLegacyMUL[i] = new UOFile(path);

                MapLegacyMUL[i].Load($"build/map{i}legacymul/{{0:D8}}.dat", false);

                path = Path.Combine(ultimaOnlineDirectory, $"map{i}xLegacyMUL.uop");

                if (File.Exists(path))
                {
                    MapXLegacyMUL[i] = new UOFile(path);
                    MapXLegacyMUL[i].Load(packedMapFilePattern, false);
                }

                path = Path.Combine(ultimaOnlineDirectory, $"statics{i}.mul");

                Statics[i] = new UOFile(path);
                Statics[i].Load("", false);

                path = Path.Combine(ultimaOnlineDirectory, $"staidx{i}.mul");

                StaticsIdx[i] = new UOFile(path);
                StaticsIdx[i].Load("", false);

                path = Path.Combine(ultimaOnlineDirectory, $"statics{i}x.mul");

                if (File.Exists(path))
                {
                    StaticsX[i] = new UOFile(path);
                    StaticsX[i].Load("", false);
                }

                path = Path.Combine(ultimaOnlineDirectory, $"staidx{i}x.mul");

                if (File.Exists(path))
                {
                    StaticsXIdx[i] = new UOFile(path);
                    StaticsXIdx[i].Load("", false);
                }
            }
        }

        public void LoadMap(int i)
        {
            MapBlocksSize[i, 0] = MapsDefaultSize[i, 0] >> 3;
            MapBlocksSize[i, 1] = MapsDefaultSize[i, 1] >> 3;

            int mapblocksize = Unsafe.SizeOf<MapBlock>();
            var staticidxblocksize = Unsafe.SizeOf<StaidxBlock>();
            var staticblocksize = Unsafe.SizeOf<StaticsBlock>();
            var width = MapBlocksSize[i, 0];
            var height = MapBlocksSize[i, 1];
            var maxblockcount = width * height;
            BlockData[i] = new IndexMap[maxblockcount];
            var file = MapLegacyMUL[i];
            var fileidx = StaticsIdx[i];
            var staticfile = Statics[i];

            ulong uopoffset = 0;
            int fileNumber = -1;
            bool isUop = file.IsUop;

            for (int block = 0; block < maxblockcount; block++)
            {
                int blocknum = block;

                if (isUop)
                {
                    blocknum &= 4095;

                    int shifted = block >> 12;

                    if (fileNumber != shifted)
                    {
                        fileNumber = shifted;

                        if (shifted < file.FileIndices.Length)
                        {
                            uopoffset = (ulong)file.FileIndices[shifted].Offset;
                        }
                    }
                }

                var mapPos = uopoffset + (ulong)(blocknum * mapblocksize);
                var staticPos = 0ul;
                var staticCount = 0u;

                fileidx.Stream!.Seek(block * staticidxblocksize, SeekOrigin.Begin);

                var st = fileidx.Reader!.Read<StaidxBlock>();

                if (st.Size > 0 && st.Position != 0xFFFF_FFFF)
                {
                    staticPos = st.Position;
                    staticCount = Math.Min(1024, (uint)(st.Size / staticblocksize));
                }

                ref var data = ref BlockData[i][block];

                data.MapAddress = mapPos;
                data.StaticAddress = staticPos;
                data.StaticCount = staticCount;
                data.OriginalMapAddress = mapPos;
                data.OriginalStaticAddress = staticPos;
                data.OriginalStaticCount = staticCount;

                data.MapFile = file;
                data.StaticFile = staticfile;
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
        public uint Position;
        public ushort Size;
        public byte Unknown;
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
        public UOFile               MapFile;
        public UOFile               StaticFile;
        public ulong                MapAddress;
        public ulong                OriginalMapAddress;
        public ulong                OriginalStaticAddress;
        public uint                 OriginalStaticCount;
        public ulong                StaticAddress;
        public uint                 StaticCount;
        public static IndexMap      Invalid = new IndexMap();
    }
}
