// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.Core;

public struct Rectangle
{
    public int X;
    public int Y;
    public int Width;
    public int Height;

    public static Rectangle Empty
    {
        get
        {
            return emptyRectangle;
        }
    }

    public Rectangle(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public bool Contains(int x, int y)
    {
        return ((X <= x) && (x < (X + Width)) && (Y <= y) && (y < (Y + Height)));
    }

    private static Rectangle emptyRectangle = new Rectangle();

}
