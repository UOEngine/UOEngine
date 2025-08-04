using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.UOAssets;

public struct LandTile
{
    public ulong Flags;
    public ushort TexID;
    public string Name;

    public LandTile(ulong flags, ushort texId, string name)
    {
        Flags = flags;
        TexID = texId;
        Name = name;
    }
}

public class TileDataDeserialiser : IUOAssetDeserialiser<LandTile[]>
{
    public LandTile[] Deserialise(BinaryReader reader)
    {
        const int landSize = 512;
        int num_land_tile_data = 580;

        const int max_land_data = 16384;

        bool isold = false;

        reader.BaseStream.Seek(0, SeekOrigin.Begin);

        var LandTiles = new LandTile[max_land_data];

        Span<byte> buf = stackalloc byte[20];

        bool process = true;

        for (int i = 0; i < 512; i++)
        {
            if(process == false)
            {
                break;
            }

            reader.ReadUInt32();

            for (int j = 0; j < 32; j++)
            {
                if (reader.BaseStream.Position + (isold ? 4 : 8) + 2 + 20 > reader.BaseStream.Length)
                {
                    process = false;

                    break;
                }

                int idx = i * 32 + j;
                ulong flags = isold ? reader.ReadUInt32() : reader.ReadUInt64();
                ushort textId = reader.ReadUInt16();

                reader.Read(buf);

                var name = string.Intern(Encoding.UTF8.GetString(buf).TrimEnd('\0'));

                LandTiles[idx] = new LandTile(flags, textId, name);
            }

        }
        
        // Need statics too.

        return LandTiles;
    }
}
