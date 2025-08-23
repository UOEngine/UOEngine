using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Interop;

[StructLayout(LayoutKind.Sequential)]
public struct IntVector2DNative
{
    public int X;
    public int Y;
}
