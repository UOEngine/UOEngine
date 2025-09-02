using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UOEngine.Core;

public enum EMatrix4x4Init
{
    Identity
}

[StructLayout(LayoutKind.Sequential)]
public struct Matrix4x4
{
    public static readonly Matrix4x4 Identity = new(EMatrix4x4Init.Identity);

    const int NumRows = 4;
    const int NumCols = 4;

    const int NumElements = NumCols * NumRows;

    // Stored as columns.
    public float M00;
    public float M10;
    public float M20;
    public float M30;

    public float M01;
    public float M11;
    public float M21;
    public float M31;

    public float M02;
    public float M12;
    public float M22;
    public float M32;

    public float M03;
    public float M13;
    public float M23;
    public float M33;

    public Vector3 Position
    {
        get { return new Vector3(M03, M13, M23); }
        set { SetColumn(3, value); }
    }

    public Matrix4x4()
    {

    }

    public Matrix4x4(EMatrix4x4Init init)
    {
        this[0, 0] = 1.0f;
        this[1, 1] = 1.0f;
        this[2, 2] = 1.0f;
        this[3, 3] = 1.0f;
    }


    public static Matrix4x4 CreateOrthographic(float left, float right, float bottom, float top, float near, float far)
    {
        Matrix4x4 matrix = Identity;

        float x = 2.0f / (right - left);
        float y = 2.0f / (top - bottom);
        float z = -2.0f / (far - near);
        float tx = -(right + left) / (right - left);
        float ty = -(top + bottom) / (top - bottom);
        float tz = -(far + near) / (far - near);

        matrix[0, 0] = x;
        matrix[0, 3] = tx;

        matrix[1, 1] = y;
        matrix[1, 3] = ty;

        matrix[2, 2] = z;
        matrix[2, 3] = tz;

        return matrix;
    }

    public float this[int index]
    {
        get
        {
            switch (index)
            {
                case 0: return M00;
                case 1: return M10;
                case 2: return M20;
                case 3: return M30;
                case 4: return M01;
                case 5: return M11;
                case 6: return M21;
                case 7: return M31;
                case 8: return M02;
                case 9: return M12;
                case 10: return M22;
                case 11: return M32;
                case 12: return M03;
                case 13: return M13;
                case 14: return M23;
                case 15: return M33;
                default:
                    throw new IndexOutOfRangeException("Invalid matrix index!");
            }
        }

        set
        {
            switch (index)
            {
                case 0: M00 = value; break;
                case 1: M10 = value; break;
                case 2: M20 = value; break;
                case 3: M30 = value; break;
                case 4: M01 = value; break;
                case 5: M11 = value; break;
                case 6: M21 = value; break;
                case 7: M31 = value; break;
                case 8: M02 = value; break;
                case 9: M12 = value; break;
                case 10: M22 = value; break;
                case 11: M32 = value; break;
                case 12: M03 = value; break;
                case 13: M13 = value; break;
                case 14: M23 = value; break;
                case 15: M33 = value; break;

                default:
                    throw new IndexOutOfRangeException("Invalid matrix index!");
            }
        }
    }

    public float this[int row, int column]
    {
        get
        {
            return this[row + 4 * column];
        }

        set
        {
            this[row + 4 * column] = value;
        }
    }

    public static Matrix4x4 Translate(Vector3 translation)
    {
        var matrix = Identity;

        matrix[0, 3] = translation.X;
        matrix[1, 3] = translation.Y;
        matrix[2, 3] = translation.Z;

        matrix[3, 3] = 1.0f;

        return matrix;
    }

    public static Matrix4x4 Rotate(Quaternion rotation)
    {

        float x = rotation.X * 2.0f;
        float y = rotation.Y * 2.0f;
        float z = rotation.Z * 2.0f;
        float xx = rotation.X * x;
        float yy = rotation.Y * y;
        float zz = rotation.Z * z;
        float xy = rotation.X * y;
        float xz = rotation.X * z;
        float yz = rotation.Y * z;
        float wx = rotation.W * x;
        float wy = rotation.W * y;
        float wz = rotation.W * z;

        Matrix4x4 m;

        m.M00 = 1.0f - (yy + zz); 
        m.M10 = xy + wz; 
        m.M20 = xz - wy; 
        m.M30 = 0.0F;
        m.M01 = xy - wz; 
        m.M11 = 1.0f - (xx + zz); 
        m.M21 = yz + wx; 
        m.M31 = 0.0F;
        m.M02 = xz + wy;
        m.M12 = yz - wx; 
        m.M22 = 1.0f - (xx + yy); 
        m.M32 = 0.0F;
        m.M03 = 0.0F; 
        m.M13 = 0.0F; 
        m.M23 = 0.0F; 
        m.M33 = 1.0F;

        return m;
    }

    public static Matrix4x4 operator* (Matrix4x4 lhs, Matrix4x4 rhs)
    {
        Matrix4x4 result = new Matrix4x4();

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                float sum = 0.0f;

                for (int k = 0; k < 4; k++)
                {
                    sum += lhs[row, k] * rhs[k, col];
                }

                result[row, col] = sum;
            }
        }

        return result;
    }

    public static void CreateLookAt(in Vector3 cameraPosition, in Vector3 cameraTarget, in Vector3 cameraUpVector, out Matrix4x4 result)
    {
        Vector3 forwardAxis = Vector3.Normalise(-cameraPosition + cameraTarget);
        Vector3 rightAxis = Vector3.Normalise(Vector3.Cross(forwardAxis, cameraUpVector.Normalise()));
        Vector3 upAxis = Vector3.Cross(forwardAxis, rightAxis);

        result = Identity;

        //rightAxis = Vector3.Forward;
        //upAxis = Vector3.Right;
        //tor3 forwardAxis = -Vector3.Up;

        result[0,0] = rightAxis.X;
        result[0,1] = upAxis.X;
        result[0,2] = forwardAxis.X;;
        result[0,3] = 0.0f;

        result[1,0] = rightAxis.Y;
        result[1,1] = upAxis.Y;
        result[1,2] = forwardAxis.Y;
        result[1,3] = 0.0f;

        result[2,0] = rightAxis.Z;
        result[2,1] = upAxis.Z;
        result[2,2] = forwardAxis.Z;
        result[2,3] = 0.0f;

        result[3,0] = -Vector3.Dot(rightAxis, cameraPosition);
        result[3,1] = -Vector3.Dot(upAxis, cameraPosition);
        result[3,2] = -Vector3.Dot(forwardAxis, cameraPosition);
        result[3,3] = 1.0f;
    }

    public ReadOnlySpan<float> AsSpan()
    {
        return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<Matrix4x4, float>(ref this), 16);
    }

    public void SetColumn(int column, Vector3 value)
    {
        this[0, column] = value.X;
        this[1, column] = value.Y;
        this[2, column] = value.Z;
    }
}
