namespace Microsoft.Xna.Framework;

public struct Vector2
{
    public static Vector2 Zero;

    private System.Numerics.Vector2 vec2;

    public Vector2(float x, float y)
    {
        vec2 = new System.Numerics.Vector2(x, y);
    }
}
