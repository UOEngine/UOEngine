using System.Numerics;

namespace Microsoft.Xna.Framework;

public struct Matrix
{
    public float M11
    {
        get => M.M11;
        set => M.M11 = value;
    }

    public float M12
    {
        get => M.M12;
        set => M.M12 = value;
    }

    public float M13
    {
        get => M.M13;
        set => M.M13 = value;
    }

    public float M14
    {
        get => M.M14;
        set => M.M14 = value;
    }

    public float M21
    {
        get => M.M21;
        set => M.M21 = value;
    }

    public float M22
    {
        get => M.M22;
        set => M.M22 = value;
    }

    public float M23
    {
        get => M.M23;
        set => M.M23 = value;
    }

    public float M24
    {
        get => M.M24;
        set => M.M24 = value;
    }

    public float M31
    {
        get => M.M31;
        set => M.M31 = value;
    }

    public float M32
    {
        get => M.M32;
        set => M.M32 = value;
    }

    public float M33
    {
        get => M.M33;
        set => M.M33 = value;
    }

    public float M34
    {
        get => M.M34;
        set => M.M34 = value;
    }

    public float M41
    {
        get => M.M41;
        set => M.M41 = value;
    }

    public float M42
    {
        get => M.M42;
        set => M.M42 = value;
    }

    public float M43
    {
        get => M.M43;
        set => M.M43 = value;
    }

    public float M44
    {
        get => M.M44;
        set => M.M44 = value;
    }

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
        var m = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, zNearPlane, zFarPlane);

        return new Matrix(m);
    }

    public static Matrix CreateTranslation(float x, float y, float z)
    {
        return new Matrix(Matrix4x4.CreateTranslation(x, y, z));
    }

    public static Matrix Multiply(in Matrix matrix1, in Matrix matrix2)
    {
        return new Matrix(matrix1.M * matrix2.M);
    }

    public static Matrix operator *(Matrix matrix1, Matrix matrix2)
    {
        return Multiply(matrix1, matrix2);
    }

    public static void Multiply(in Matrix matrix1, in Matrix matrix2, out Matrix result)
    {
        result = matrix1 * matrix2;
    }
}
