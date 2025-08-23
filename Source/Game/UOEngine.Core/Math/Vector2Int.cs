using UOEngine.Interop;

namespace UOEngine.Core;

public struct Vector2Int
{
    public int X;
    public int Y;

    public static Vector2Int Zero = new(0, 0);

    public Vector2Int(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static implicit operator Vector2Int(IntVector2DNative intVector2D)
    {
        return new Vector2Int(intVector2D.X, intVector2D.Y);
    }
}
