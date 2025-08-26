using System.Runtime.InteropServices;

namespace UOEngine
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Colour
    {
        public Colour(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Colour(uint colour)
        {
            R = (byte)((colour >> 0)  & 0xFF);
            G = (byte)((colour >> 8)  & 0xFF);
            B = (byte)((colour >> 16) & 0xFF);
            A = (byte)((colour >> 24) & 0xFF);
        }

        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public static readonly Colour Red = new(255, 0, 0, 255);
    }
}
