namespace Microsoft.Xna.Framework.Graphics;

public abstract class GraphicsResource: IDisposable
{
    protected string? _Name;

    public virtual string? Name
    {
        get
        {
            return _Name;
        }
        set
        {
            _Name = value;
        }
    }

    public void Dispose()
    {
    }

    protected virtual void Dispose(bool disposing)
    {
    }
}
