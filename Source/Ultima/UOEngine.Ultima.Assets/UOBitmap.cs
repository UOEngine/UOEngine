#pragma warning disable CS0660, CS0661
using System.Runtime.InteropServices;

namespace UOEngine.Ultima.UOAssets;

public ref struct UOBitmap
{
    public Span<uint> Texels;
    public uint Width = 0;
    public uint Height = 0;

    public bool IsEmpty => this == Empty();

    public UOBitmap(uint width, uint height, Span<uint> texels)
    {
        Width = width;
        Height = height;
        Texels = texels;
    }

    public static bool operator ==(UOBitmap left, UOBitmap right)
    {
        if (left.Width != right.Height)
        {
            return false;
        }

        if (left.Height != right.Height)
        {
            return false;
        }

        if (left.Texels == right.Texels)
        {
            return true;
        }

        return true;
    }

    public static bool operator !=(UOBitmap left, UOBitmap right)
    {
        return !(left == right);
    }

    public void DeserialiseFromUOPackageFileResource(byte[] data)
    {
        using var reader = new BinaryReader(new MemoryStream(data));

        Width = reader.ReadUInt32();
        Height = reader.ReadUInt32();

        //if (info.Width <= 0)
        //{
        //    info.Width = (int)width;
        //}

        //if (info.Height <= 0)
        //{
        //    info.Height = (int)height;
        //}

        Texels = new uint[Width * Height];
        var len = reader.BaseStream.Length - reader.BaseStream.Position;
        var halfLen = len >> 2;
        var start = reader.BaseStream.Position;
        var rowLookup = new int[Height];

        reader.Read(MemoryMarshal.AsBytes<int>(rowLookup.AsSpan()));

        ushort color = 0;// info.Hue;

        for (var y = 0; y < Height; ++y)
        {
            reader.BaseStream.Position = (start + (rowLookup[y] << 2));

            var pixelIndex = (int)(y * Width);
            var gsize = (y < Height - 1) ? rowLookup[y + 1] - rowLookup[y] : halfLen - rowLookup[y];

            for (var i = 0; i < gsize; ++i)
            {
                var value = reader.ReadUInt16();
                var run = reader.ReadUInt16();
                var rbga = 0u;

                if (color != 0 && value != 0)
                {
                    throw new NotImplementedException();
                    //value = FileManager.Hues.GetColor16(value, color);
                }

                if (value != 0)
                {
                    rbga = HuesHelper.Color16To32(value) | 0xFF_00_00_00;
                }

                Texels.Slice(pixelIndex, run).Fill(rbga);
                pixelIndex += run;
            }
        }
    }

    public static UOBitmap Empty()
    {
        return new UOBitmap(0, 0, []);
    }
}
