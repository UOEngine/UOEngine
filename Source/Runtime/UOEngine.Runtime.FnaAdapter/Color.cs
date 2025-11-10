namespace Microsoft.Xna.Framework;


public struct Color
{
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
