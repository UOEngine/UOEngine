using System.Diagnostics;

namespace Microsoft.Xna.Framework;

[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct Vector4
{
    public float X
    {
        get => Vec4.X;
        set => Vec4.X = value;
    }

    public float Y
    {
        get => Vec4.Y;
        set => Vec4.Y = value;
    }

    public float Z
    {
        get => Vec4.Z;
        set => Vec4.Z = value;
    }

    public float W
    {
        get => Vec4.W;
        set => Vec4.W = value;
    }

    public static Vector4 Zero => new(System.Numerics.Vector4.Zero);

    public Vector4(float x, float y, float z, float w)
    {
        Vec4 = new System.Numerics.Vector4(x, y, z, w );
    }

    public Vector4(in System.Numerics.Vector4 vector4)
    {
        Vec4 = vector4;
    }

    internal string DebugDisplayString
    {
        get
        {
            return string.Concat(
                X.ToString(), " ",
                Y.ToString(), " ",
                Z.ToString(), " ",
                W.ToString()
            );
        }
    }

    public System.Numerics.Vector4 Vec4;
}
