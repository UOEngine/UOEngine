using System.Diagnostics;

namespace UOEngine.Runtime.Core;

[DebuggerDisplay("{R}, {G}, {B}, {A}")]
public struct Colour
{
    public byte R;
    public byte G;
    public byte B;
    public byte A;

    public static Colour Red = new(255, 0, 0);
    public static Colour White = new(255, 255, 255);

    public Colour(byte r, byte g, byte b, byte a = 255)
    {
        R = r; 
        G = g; 
        B = b; 
        A = a;
    }
}
