namespace Microsoft.Xna.Framework.Graphics;

public class BlendState
{
    public static readonly BlendState AlphaBlend;
    public static readonly BlendState NonPremultiplied;
    public static readonly BlendState Additive;

    public Blend ColorSourceBlend;
    public Blend ColorDestinationBlend;
    public BlendFunction ColorBlendFunction;
}