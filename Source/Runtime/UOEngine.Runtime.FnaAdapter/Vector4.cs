namespace Microsoft.Xna.Framework;

public struct Vector4
{
    public Vector4(float x, float y, float z, float w)
    {
        Vec4 = new System.Numerics.Vector4(x, y, z, w );
    }

    public System.Numerics.Vector4 Vec4;
}
