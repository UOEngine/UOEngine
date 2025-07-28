using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Editor;

struct Vector4
{
    float X;
    float Y;
    float Z;
    float W;
}

internal class Matrix4x4
{
    const int NumRows = 4;
    const int NumCols = 4;

    const int NumElements = NumCols * NumRows;

    private float[] _elements = new float[NumElements];

    Matrix4x4(Vector4 column0, Vector4 column1, Vector4 column2, Vector4 column4)
    {

    }
}
