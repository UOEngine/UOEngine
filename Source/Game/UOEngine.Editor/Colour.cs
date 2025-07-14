using System.Runtime.InteropServices;

namespace UOEngine
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Colour
    {
        Colour(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        byte R;
        byte G;
        byte B;
        byte A;

        public static readonly Colour Red = new(255, 0, 0, 255);
    }
}
