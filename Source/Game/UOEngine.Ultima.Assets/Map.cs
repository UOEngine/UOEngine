using System.Runtime.CompilerServices;

using UOEngine.PackageFile;

namespace UOEngine.UOAssets;

internal class Map
{
    public IndexMap[] BlockData { get; private set; } = new IndexMap[1];

    private int _blockSizeX = 0;
    private int _blockSizeY = 0;

    public void Deserialise(int sizeX, int sizeY, UOPackageFile mapFile, bool isUop, BinaryReader fileIdxReader)
    {
        _blockSizeX = sizeX >> 3;
        _blockSizeY = sizeY >> 3;

        int mapblocksize = Unsafe.SizeOf<MapBlock>();
        var staticidxblocksize = Unsafe.SizeOf<StaidxBlock>();
        var staticblocksize = Unsafe.SizeOf<StaticsBlock>();
        var width = _blockSizeX;
        var height = _blockSizeY;
        var maxblockcount = width * height;

        BlockData = new IndexMap[maxblockcount];

        ulong uopoffset = 0;
        int fileNumber = -1;
        //bool isUop = file.IsUop;

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

                    if (shifted < mapFile.FileIndices.Length)
                    {
                        uopoffset = (ulong)mapFile.FileIndices[shifted].Offset;
                    }
                }
            }

            var mapPos = uopoffset + (ulong)(blocknum * mapblocksize);
            var staticPos = 0ul;
            var staticCount = 0u;

            fileIdxReader.BaseStream.Seek(block * staticidxblocksize, SeekOrigin.Begin);

            var st = fileIdxReader.Read<StaidxBlock>();

            if (st.Size > 0 && st.Position != 0xFFFF_FFFF)
            {
                staticPos = st.Position;
                staticCount = Math.Min(1024, (uint)(st.Size / staticblocksize));
            }

            ref var data = ref BlockData[block];

            data.MapAddress = mapPos;
            data.StaticAddress = staticPos;
            data.StaticCount = staticCount;
            data.OriginalMapAddress = mapPos;
            data.OriginalStaticAddress = staticPos;
            data.OriginalStaticCount = staticCount;

            //data.MapFile = file;
            //data.StaticFile = staticfile;
        }
    }
}
