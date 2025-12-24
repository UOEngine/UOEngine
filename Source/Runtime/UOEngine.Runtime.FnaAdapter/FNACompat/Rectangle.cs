using UoeRectangle = UOEngine.Runtime.Core.Rectangle;

namespace Microsoft.Xna.Framework;

public struct Rectangle
{
    public int Width => _rectangle.Width;
    public int Height => _rectangle.Height;
    public int X => _rectangle.X;
    public int Y => _rectangle.Y;

    public static Rectangle Empty
    {
        get
        {
            return emptyRectangle;
        }
    }

    private UoeRectangle _rectangle;


    public Rectangle(int x, int y, int width, int height)
    {
        _rectangle = new UoeRectangle(x, y, width, height);
    }

    public bool Contains(int x, int y)
    {
        return _rectangle.Contains(x, y);
    }

    private static Rectangle emptyRectangle = new();

}
