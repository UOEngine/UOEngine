using System.Diagnostics;

namespace UOEngine.Core;

[DebuggerDisplay("X = {X} Y = {Y} Z = {Z}")]
public struct Vector3
{
    public float X;
    public float Y;
    public float Z;

    public static readonly Vector3 Forward = new(1.0f, 0.0f, 0.0f);
    public static readonly Vector3 Up = new(0.0f, 0.0f, 1.0f);
    public static readonly Vector3 Right = new(0.0f, 1.0f, 0.0f);

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vector3 operator -(Vector3 value)
    {
        value = new Vector3(-value.X, -value.Y, -value.Z);

        return value;
    }

}
