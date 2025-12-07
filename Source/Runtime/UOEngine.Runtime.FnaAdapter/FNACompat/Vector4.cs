using System.Diagnostics;

namespace Microsoft.Xna.Framework;

[DebuggerDisplay("{DebugDisplayString,nq}")]
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
