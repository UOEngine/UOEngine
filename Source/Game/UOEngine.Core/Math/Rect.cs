using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Core;


[StructLayout(LayoutKind.Sequential)]
public struct Rect
{
    public uint Left;
    public uint Top;
    public uint Width;
    public uint Height;

    public readonly uint Right => Left + Width;
    public uint Bottom => Top + Height;
}
