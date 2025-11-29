namespace Microsoft.Xna.Framework;

public struct Vector4
{
    public float X => Vec4.X;

    public float Y => Vec4.Y;

    public float Z => Vec4.Z;

    public float W => Vec4.W;

    public Vector4(float x, float y, float z, float w)
    {
        Vec4 = new System.Numerics.Vector4(x, y, z, w );
    }

    public System.Numerics.Vector4 Vec4;
}
