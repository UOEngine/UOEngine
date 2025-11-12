namespace Microsoft.Xna.Framework;


public struct Color
{
    public byte B;
    public byte G;
    public byte R;
    public byte A;

    public static Color Black;
    public static Color White;

    static Color()
    {
        White = new Color(uint.MaxValue);
    }

    private Color(uint packedValue)
    {
        this.packedValue = packedValue;
    }

    private uint packedValue;

}
