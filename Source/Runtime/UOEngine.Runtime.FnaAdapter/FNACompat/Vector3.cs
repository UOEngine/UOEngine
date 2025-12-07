using System.Diagnostics;

namespace Microsoft.Xna.Framework;

[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct Vector3
{
    public float X;
    public float Y;
    public float Z;

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    internal string DebugDisplayString
    {
        get
        {
            return string.Concat(
                X.ToString(), " ",
                Y.ToString(), " ",
                Z.ToString()
            );
        }
    }
}
