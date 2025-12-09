using System.Xml.Linq;

namespace Microsoft.Xna.Framework.Graphics;

public class BlendState
{
    public static readonly BlendState AlphaBlend = new BlendState(
        "BlendState.AlphaBlend",
        Blend.One,
        Blend.One,
        Blend.InverseSourceAlpha,
        Blend.InverseSourceAlpha
    );

    public static readonly BlendState Additive = new BlendState(
        "BlendState.Additive",
        Blend.SourceAlpha,
        Blend.SourceAlpha,
        Blend.One,
        Blend.One
    );

    public static readonly BlendState NonPremultiplied = new BlendState(
        "BlendState.NonPremultiplied",
        Blend.SourceAlpha,
        Blend.SourceAlpha,
        Blend.InverseSourceAlpha,
        Blend.InverseSourceAlpha
    );

    public static readonly BlendState Opaque = new BlendState(
        "BlendState.Opaque",
        Blend.One,
        Blend.One,
        Blend.Zero,
        Blend.Zero
    );

    public Blend AlphaDestinationBlend;
    public Blend AlphaSourceBlend;
    public Color BlendFactor;
    public Blend ColorSourceBlend;
    public Blend ColorDestinationBlend;

    public BlendFunction AlphaBlendFunction;
    public BlendFunction ColorBlendFunction;

    public ColorWriteChannels ColorWriteChannels;
    public ColorWriteChannels ColorWriteChannels1;
    public ColorWriteChannels ColorWriteChannels2;
    public ColorWriteChannels ColorWriteChannels3;

    public int MultiSampleMask;

    public string Name { get; private set; }

    public BlendState()
    {
        AlphaBlendFunction = BlendFunction.Add;
        AlphaDestinationBlend = Blend.Zero;
        AlphaSourceBlend = Blend.One;
        ColorBlendFunction = BlendFunction.Add;
        ColorDestinationBlend = Blend.Zero;
        ColorSourceBlend = Blend.One;
        ColorWriteChannels = ColorWriteChannels.All;
        ColorWriteChannels1 = ColorWriteChannels.All;
        ColorWriteChannels2 = ColorWriteChannels.All;
        ColorWriteChannels3 = ColorWriteChannels.All;
        BlendFactor = Color.White;
        MultiSampleMask = -1; // AKA 0xFFFFFFFF
    }

    private BlendState(
        string name,
        Blend colorSourceBlend,
        Blend alphaSourceBlend,
        Blend colorDestBlend,
        Blend alphaDestBlend
    ) : this()
    {
        Name = name;
        ColorSourceBlend = colorSourceBlend;
        AlphaSourceBlend = alphaSourceBlend;
        ColorDestinationBlend = colorDestBlend;
        AlphaDestinationBlend = alphaDestBlend;
    }

}