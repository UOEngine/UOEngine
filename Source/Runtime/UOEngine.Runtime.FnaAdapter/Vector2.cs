namespace Microsoft.Xna.Framework;

public struct Vector2
{
    public static Vector2 Zero;

    public float X 
    {
        get => _vec2.X; 
        set => _vec2.X = value;
    }

    public float Y
    {
        get => _vec2.Y;
        set => _vec2.Y = value;
    }

    private System.Numerics.Vector2 _vec2;

    public Vector2(float x, float y)
    {
        _vec2 = new System.Numerics.Vector2(x, y);
    }
}
