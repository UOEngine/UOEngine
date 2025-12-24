using System.Diagnostics;

namespace Microsoft.Xna.Framework;

[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct Vector3
{
    public static Vector3 Zero
    {
        get
        {
            return zero;
        }
    }

    public static Vector3 One
    {
        get
        {
            return one;
        }
    }

    public float X
    {
        get => vector3.X; 
        set => vector3.X = value;
    }

    public float Y
    {
        get => vector3.Y;
        set => vector3.Y = value;
    }

    public float Z
    {
        get => vector3.Z;
        set => vector3.Z = value;
    }

    private System.Numerics.Vector3 vector3;

    public Vector3(float x, float y, float z)
    {
        vector3 = new System.Numerics.Vector3(x, y, z);
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

    private static Vector3 one = new Vector3(1f, 1f, 1f);
    private static Vector3 zero = new Vector3(0f, 0f, 0f);

}
