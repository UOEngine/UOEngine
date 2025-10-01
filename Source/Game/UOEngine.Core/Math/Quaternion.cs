using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Core;

[DebuggerDisplay("X = {X} Y = {Y} Z = {Z} W = {W}")]
public struct Quaternion
{
    public static readonly Quaternion Identity = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

    public float X;
    public float Y;
    public float Z;
    public float W;

    public Vector3 ForwardVector => Rotate(Vector3.Forward);
    public Vector3 RightVector => Rotate(Vector3.Right);
    public Vector3 UpVector => Rotate(Vector3.Up);

    public Quaternion()
    {
        this = Identity;
    }

    public Quaternion(float x, float y, float z, float w) 
    {
        X = x; 
        Y = y; 
        Z = z; 
        W = w;
    }

    public Quaternion(in Vector3 axis, float angle)
    {
        float half = angle * 0.5f;
        float sin = -MathF.Sin(half);
        float cos = MathF.Cos(half);

        X = axis.X * sin;
        Y = axis.Y * sin;
        Z = axis.Z * sin;
        W = cos;

        NormaliseInPlace();
    }

    public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
    {
        float half = angle * 0.5f;
        float sin = -MathF.Sin(half);
        float cos = MathF.Cos(half);

        var quat = new Quaternion(axis.X * sin, axis.Y * sin, axis.Z * sin, cos);

        quat.NormaliseInPlace();

        return quat;
    }

    public static Quaternion Inverse(Quaternion value)
    {
        Debug.Assert(value.IsNormalised());

        Quaternion inverse = new Quaternion(-value.X, -value.Y, -value.Z, value.W);

        return inverse;
    }

    public float LengthSquared()
    {
        return X * X + Y * Y +  Z * Z + W * W;
    }

    public void NormaliseInPlace()
    {
        float length = MathF.Sqrt(LengthSquared());

        X = X / length;
        Y = Y / length;
        Z = Z / length;
        W = W / length;
    }

    public bool IsNormalised()
    {
        return Math.Abs(LengthSquared() - 1.0f) < 1e-4;
    }

    public static Quaternion operator *(in Quaternion lhs, in Quaternion rhs)
    {
        Debug.Assert(lhs.IsNormalised());
        Debug.Assert(rhs.IsNormalised());

        float x = rhs.W * lhs.X + rhs.X * lhs.W + rhs.Y * lhs.Z - rhs.Z * lhs.Y;
        float y = rhs.W * lhs.Y - rhs.X * lhs.Z + rhs.Y * lhs.W + rhs.Z * lhs.X;
        float z = rhs.W * lhs.Z + rhs.X * lhs.Y - rhs.Y * lhs.X + rhs.Z * lhs.W;
        float w = rhs.W * lhs.W - rhs.X * lhs.X - rhs.Y * lhs.Y - rhs.Z * lhs.Z;

        return new Quaternion(x, y, w, z);
    }

    public Rotator ToEulerAngles()
    {
        // pitch (X-axis rotation)
        float sinp = 2 * (W * X + Y * Z);
        float cosp = 1 - 2 * (X * X + Y * Y);
        float pitch = MathF.Atan2(sinp, cosp);

        // yaw (Y-axis rotation)
        float siny = 2 * (W * Y - Z * X);
        siny = Math.Clamp(siny, -1f, 1f); // numerical safety
        float yaw = MathF.Asin(siny);

        // roll (Z-axis rotation)
        float sinr = 2 * (W * Z + X * Y);
        float cosr = 1 - 2 * (Y * Y + Z * Z);
        float roll = MathF.Atan2(sinr, cosr);

        return new Rotator(pitch, roll, yaw);
    }

    public Vector3 Rotate(in Vector3 vector)
    {
        var q = new Vector3(X, Y, Z);

        var tt = 2.0f * Vector3.Cross(q, vector);
        var result = vector + W * tt + Vector3.Cross(q, tt);

        return result;
    }
}
