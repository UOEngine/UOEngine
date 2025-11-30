#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */

/* Derived from code by the Mono.Xna Team (Copyright 2006).
 * Released under the MIT License. See monoxna.LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework;

public struct Color
{
    public byte B;
    public byte G;
    public byte R;
    public byte A;

    public static Color Black;
    public static Color White;

    public Color(float r, float g, float b)
    {
        throw new NotImplementedException();
    }

    public Color(float r, float g, float b, float alpha)
    {
        throw new NotImplementedException();
    }

    static Color()
    {
        White = new Color(uint.MaxValue);
    }

    public static bool operator == (Color a, Color b)
    {
        return (a.A == b.A &&
                a.R == b.R &&
                a.G == b.G &&
                a.B == b.B);
    }

    public static bool operator !=(Color a, Color b)
    {
        return !(a == b);
    }

    private Color(uint packedValue)
    {
        this.packedValue = packedValue;
    }

    private uint packedValue;

}
