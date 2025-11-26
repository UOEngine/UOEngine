using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Runtime.RHI;

public struct RhiViewport
{
    public int Width;
    public int Height;
    public int X;
    public int Y;

    public RhiViewport(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}
