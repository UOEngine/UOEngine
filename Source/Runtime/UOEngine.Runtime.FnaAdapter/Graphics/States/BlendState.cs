using System.Xml.Linq;

namespace Microsoft.Xna.Framework.Graphics;

public class BlendState
{
    public static readonly BlendState NonPremultiplied = new BlendState(
    "BlendState.NonPremultiplied",
    Blend.SourceAlpha,
    Blend.SourceAlpha,
    Blend.InverseSourceAlpha,
    Blend.InverseSourceAlpha
);

    private BlendState(string name, Blend colorSourceBlend, Blend alphaSourceBlend, Blend colorDestBlend, Blend alphaDestBlend) 
    {
        Name = name;
        ColorSourceBlend = colorSourceBlend;
        AlphaSourceBlend = alphaSourceBlend;
        ColorDestinationBlend = colorDestBlend;
        AlphaDestinationBlend = alphaDestBlend;
    }
}
