using System.Diagnostics;
using System.Numerics;

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
    public static readonly Vector3 Zero = new(0.0f, 0.0f, 0.0f);

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vector3(Quaternion quat)
    {
        X = quat.X;
        Y = quat.Y;
        Z = quat.Z;
    }

    public static Vector3 operator -(Vector3 value)
    {
        value = new Vector3(-value.X, -value.Y, -value.Z);

        return value;
    }
    public static Vector3 operator *(float scaleFactor, Vector3 value)
    {
        value.X *= scaleFactor;
        value.Y *= scaleFactor;
        value.Z *= scaleFactor;

        return value;
    }

    public static Vector3 operator +(Vector3 Lhs, Vector3 Rhs)
    {
        return new Vector3(Lhs.X + Rhs.X, Lhs.Y + Rhs.Y, Lhs.Z + Rhs.Z);
    }

    public static Vector3 operator -(Vector3 Lhs, Vector3 Rhs)
    {
        return new Vector3(Lhs.X - Rhs.X, Lhs.Y - Rhs.Y, Lhs.Z - Rhs.Z);
    }

    public Vector3 Rotate(Quaternion rotation)
    {
        Debug.Assert(rotation.IsNormalised());

        Vector3 vectorToRotate = new Vector3(X, Y, Z);

        Vector3 qVec = new Vector3(rotation);

        Vector3 t = 2.0f * Cross(qVec, this);

        return this + rotation.W * t + Cross(qVec, t);
    }

    public static float Dot(in Vector3 A, in Vector3 B)
    {
        return A.X * B.X + A.Y * B.Y + A.Z * B.Z;
    }

    public static Vector3 Cross(in Vector3 A, in Vector3 B)
    {
        float x = A.Y * B.Z - B.Y * A.Z;
        float y = -(A.X * B.Z - B.X * A.Z);
        float z = A.X * B.Y - B.X * A.Y;

        return new Vector3(x, y, z);
    }

    public static Vector3 Normalise(in Vector3 vector)
    {
        float length = vector.Length();

        return new Vector3(vector.X / length, vector.Y / length, vector.Z / length);
    }

    public float Length()
    {
        return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
    }

    public Vector3 Normalise()
    {
        return Normalise(this);
    }
}
