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

using System.Diagnostics;
using UOEngine.Runtime.Core;

namespace Microsoft.Xna.Framework;

[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct Color : IEquatable<Color>
{
    public byte B
    {
        get
        {
            unchecked
            {
                return (byte)(this.packedValue >> 16);
            }
        }
        set
        {
            this.packedValue = (this.packedValue & 0xff00ffff) | ((uint)value << 16);
        }
    }

    public byte G
    {
        get
        {
            unchecked
            {
                return (byte)(this.packedValue >> 8);
            }
        }
        set
        {
            this.packedValue = (this.packedValue & 0xffff00ff) | ((uint)value << 8);
        }
    }

    public byte R
    {
        get
        {
            unchecked
            {
                return (byte)(this.packedValue);
            }
        }
        set
        {
            this.packedValue = (this.packedValue & 0xffffff00) | value;
        }
    }

    public byte A
    {
        get
        {
            unchecked
            {
                return (byte)(this.packedValue >> 24);
            }
        }
        set
        {
            this.packedValue = (this.packedValue & 0x00ffffff) | ((uint)value << 24);
        }
    }

    public UInt32 PackedValue
    {
        get
        {
            return packedValue;
        }
        set
        {
            packedValue = value;
        }
    }

    public static Color Black;
    public static Color White;

    public Color(float r, float g, float b)
    {
        packedValue = 0;

        R = (byte)MathHelper.Clamp(r * 255, Byte.MinValue, Byte.MaxValue);
        G = (byte)MathHelper.Clamp(g * 255, Byte.MinValue, Byte.MaxValue);
        B = (byte)MathHelper.Clamp(b * 255, Byte.MinValue, Byte.MaxValue);
        A = 255;
    }

    public Color(float r, float g, float b, float alpha)
    {
        packedValue = 0;

        R = (byte)MathHelper.Clamp(r * 255, Byte.MinValue, Byte.MaxValue);
        G = (byte)MathHelper.Clamp(g * 255, Byte.MinValue, Byte.MaxValue);
        B = (byte)MathHelper.Clamp(b * 255, Byte.MinValue, Byte.MaxValue);
        A = (byte)MathHelper.Clamp(alpha * 255, Byte.MinValue, Byte.MaxValue);
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
    public bool Equals(Color other)
    {
        return this.PackedValue == other.PackedValue;
    }

    public override bool Equals(object? obj)
    {
        return ((obj is Color) && this.Equals((Color)obj));
    }

    public override int GetHashCode()
    {
        return this.packedValue.GetHashCode();
    }

    private Color(uint packedValue)
    {
        this.packedValue = packedValue;
    }

    internal string DebugDisplayString
    {
        get
        {
            return string.Concat(
                R.ToString(), " ",
                G.ToString(), " ",
                B.ToString(), " ",
                A.ToString()
            );
        }
    }

    private uint packedValue;
}
