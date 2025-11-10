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
