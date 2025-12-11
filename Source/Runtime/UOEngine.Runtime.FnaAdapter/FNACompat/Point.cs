namespace Microsoft.Xna.Framework;

public struct Point
{
    public static Point Zero
    {
        get
        {
            return zeroPoint;
        }
    }

    private static Point zeroPoint = new Point();
}
