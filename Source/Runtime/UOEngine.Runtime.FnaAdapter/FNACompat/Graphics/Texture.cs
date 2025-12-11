using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

public class Texture
{
    public IRenderTexture RhiTexture { get;  init; }

    public SurfaceFormat Format
    {
        get;
        protected set;
    }

}
