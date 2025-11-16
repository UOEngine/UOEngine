using System.Numerics;

namespace Microsoft.Xna.Framework;

public struct Matrix
{
    public Matrix(in Matrix4x4 matrix)
    {
        M = matrix;
    }

    public static Matrix Identity = new(Matrix4x4.Identity);

    public Matrix4x4 M;

    public Matrix(
    float m11, float m12, float m13, float m14,
    float m21, float m22, float m23, float m24,
    float m31, float m32, float m33, float m34,
    float m41, float m42, float m43, float m44
    )
    {
        M = new Matrix4x4(m11, m12, m13, m14,
                          m21, m22, m23, m24,
                          m31, m32, m33, m34,
                          m41, m42, m43, m44);
    }

    public static Matrix CreateOrthographicOffCenter(
        float left,
        float right,
        float bottom,
        float top,
        float zNearPlane,
        float zFarPlane
    )
    {
        var m = Matrix4x4.CreatePerspectiveOffCenter(left, right, bottom, top, zNearPlane, zFarPlane);

        return new Matrix(m);
    }
}
