using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

public class Texture: GraphicsResource
{
    public IRenderTexture RhiTexture { get; protected set; } = null!;

    public SurfaceFormat Format
    {
        get;
        protected set;
    }

}
