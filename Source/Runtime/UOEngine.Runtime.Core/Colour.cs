// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
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
    public static Colour Green = new(0, 255, 0);
    public static Colour Blue = new(0, 0, 255);
    public static Colour White = new(255, 255, 255);

    public Colour(byte r, byte g, byte b, byte a = 255)
    {
        R = r; 
        G = g; 
        B = b; 
        A = a;
    }
}
