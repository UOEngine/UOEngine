using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UOEngine;

public struct Vector4
{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public Vector4(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }
}

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
}

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

}
