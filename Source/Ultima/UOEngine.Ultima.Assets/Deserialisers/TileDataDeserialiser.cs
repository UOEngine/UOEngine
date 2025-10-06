using System.Text;
using UOEngine.Ultima.PackageFile;

namespace UOEngine.Ultima.UOAssets;

[Flags]
public enum TileFlag : ulong
{
    /// <summary>
    ///     Nothing is flagged.
    /// </summary>
    None = 0x00000000,
    /// <summary>
    ///     Not yet documented.
    /// </summary>
    Background = 0x00000001,
    /// <summary>
    ///     Not yet documented.
    /// </summary>
    Weapon = 0x00000002,
    /// <summary>
    ///     Not yet documented.
    /// </summary>
    Transparent = 0x00000004,
    /// <summary>
    ///     The tile is rendered with partial alpha-transparency.
    /// </summary>
    Translucent = 0x00000008,
    /// <summary>
    ///     The tile is a wall.
    /// </summary>
    Wall = 0x00000010,
    /// <summary>
    ///     The tile can cause damage when moved over.
    /// </summary>
    Damaging = 0x00000020,
    /// <summary>
    ///     The tile may not be moved over or through.
    /// </summary>
    Impassable = 0x00000040,
    /// <summary>
    ///     Not yet documented.
    /// </summary>
    Wet = 0x00000080,
    /// <summary>
    ///     Unknown.
    /// </summary>
    Unknown1 = 0x00000100,
    /// <summary>
    ///     The tile is a surface. It may be moved over, but not through.
    /// </summary>
    Surface = 0x00000200,
    /// <summary>
    ///     The tile is a stair, ramp, or ladder.
    /// </summary>
    Bridge = 0x00000400,
    /// <summary>
    ///     The tile is stackable
    /// </summary>
    Generic = 0x00000800,
    /// <summary>
    ///     The tile is a window. Like <see cref="TileFlag.NoShoot" />, tiles with this flag block line of sight.
    /// </summary>
    Window = 0x00001000,
    /// <summary>
    ///     The tile blocks line of sight.
    /// </summary>
    NoShoot = 0x00002000,
    /// <summary>
    ///     For single-amount tiles, the string "a " should be prepended to the tile name.
    /// </summary>
    ArticleA = 0x00004000,
    /// <summary>
    ///     For single-amount tiles, the string "an " should be prepended to the tile name.
    /// </summary>
    ArticleAn = 0x00008000,
    /// <summary>
    ///     Not yet documented.
    /// </summary>
    Internal = 0x00010000,
    /// <summary>
    ///     The tile becomes translucent when walked behind. Boat masts also have this flag.
    /// </summary>
    Foliage = 0x00020000,
    /// <summary>
    ///     Only gray pixels will be hued
    /// </summary>
    PartialHue = 0x00040000,
    /// <summary>
    ///     Unknown.
    /// </summary>
    NoHouse = 0x00080000,
    /// <summary>
    ///     The tile is a map--in the cartography sense. Unknown usage.
    /// </summary>
    Map = 0x00100000,
    /// <summary>
    ///     The tile is a container.
    /// </summary>
    Container = 0x00200000,
    /// <summary>
    ///     The tile may be equiped.
    /// </summary>
    Wearable = 0x00400000,
    /// <summary>
    ///     The tile gives off light.
    /// </summary>
    LightSource = 0x00800000,
    /// <summary>
    ///     The tile is animated.
    /// </summary>
    Animation = 0x01000000,
    /// <summary>
    ///     Gargoyles can fly over
    /// </summary>
    NoDiagonal = 0x02000000,
    /// <summary>
    ///     Unknown.
    /// </summary>
    Unknown2 = 0x04000000,
    /// <summary>
    ///     Not yet documented.
    /// </summary>
    Armor = 0x08000000,
    /// <summary>
    ///     The tile is a slanted roof.
    /// </summary>
    Roof = 0x10000000,
    /// <summary>
    ///     The tile is a door. Tiles with this flag can be moved through by ghosts and GMs.
    /// </summary>
    Door = 0x20000000,
    /// <summary>
    ///     Not yet documented.
    /// </summary>
    StairBack = 0x40000000,
    /// <summary>
    ///     Not yet documented.
    /// </summary>
    StairRight = 0x80000000,
    /// Blend Alphas, tile blending.
    AlphaBlend = 0x0100000000,
    /// Uses new art style?
    UseNewArt = 0x0200000000,
    /// Has art being used?
    ArtUsed = 0x0400000000,
    /// Disallow shadow on this tile, lightsource? lava?
    NoShadow = 0x1000000000,
    /// Let pixels bleed in to other tiles? Is this Disabling Texture Clamp?
    PixelBleed = 0x2000000000,
    /// Play tile animation once.
    PlayAnimOnce = 0x4000000000,
    /// Movable multi? Cool ships and vehicles etc?
    MultiMovable = 0x10000000000
}

public struct LandTile
{
    public TileFlag Flags;
    public ushort TexID;
    public string Name;

    public LandTile(ulong flags, ushort texId, string name)
    {
        Flags = (TileFlag)flags;
        TexID = texId;
        Name = name;
    }

    public bool IsWet => (Flags & TileFlag.Wet) != 0;
    public bool IsImpassable => (Flags & TileFlag.Impassable) != 0;
    public bool IsNoDiagonal => (Flags & TileFlag.NoDiagonal) != 0;
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
