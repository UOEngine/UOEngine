using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Core;

public struct Quaternion
{
    public static readonly Quaternion Identity = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

    public float X;
    public float Y;
    public float Z;
    public float W;

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

    public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
    {
        float half = angle * 0.5f;
        float sin = MathF.Sin(half);
        float cos = MathF.Cos(half);

        return new Quaternion(axis.X * sin, axis.Y * sin, axis.Z * sin, cos);
    }

}
