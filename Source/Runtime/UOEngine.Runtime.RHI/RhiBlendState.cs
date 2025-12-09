namespace UOEngine.Runtime.RHI;

public enum RhiBlendOperation
{
    Add,
    Subtract,
}

public enum RhiBlendFactor
{
    Zero,
    One,
    SourceAlpha,
    InverseSourceAlpha
}

[Flags]
public enum RhiColourMask
{
    None = 0,
    R,
    G,
    B,
    A,
    RG = R | G,
    RB = R | B,
    GB = G | B,
    BA = B | A,
    RGB = R | G | B,
    RGBA = RGB | A
}

// Result colour = source_colour * source_factor <blend_operation> destination_colour * destination_factor
public record struct RhiBlendState
{
    public RhiBlendFactor SourceColourFactor;
    public RhiBlendFactor SourceAlphaFactor;

    public RhiBlendOperation ColourBlendOp;
    public RhiBlendOperation AlphaBlendOp;

    public RhiBlendFactor DestinationColourFactor;
    public RhiBlendFactor DestinationAlphaFactor;

    public RhiColourMask WriteMask;

    public string Name { get; private set; }

    public static readonly RhiBlendState Opaque = new("Opaque", RhiBlendFactor.One, RhiBlendFactor.One, RhiBlendFactor.Zero, RhiBlendFactor.Zero);

    public static readonly RhiBlendState NonPremultiplied = new("NonPremultiplied",
                                                                RhiBlendFactor.SourceAlpha, RhiBlendFactor.SourceAlpha,
                                                                RhiBlendFactor.InverseSourceAlpha, RhiBlendFactor.InverseSourceAlpha);

    public static readonly RhiBlendState AlphaBlend = new("AlphaBlend",
                                                          RhiBlendFactor.One, RhiBlendFactor.One,
                                                          RhiBlendFactor.InverseSourceAlpha, RhiBlendFactor.InverseSourceAlpha);

    public RhiBlendState()
    {
        // Just adds the source to overwrite the destination completely by default.
        SourceColourFactor = RhiBlendFactor.One;
        SourceAlphaFactor = RhiBlendFactor.One;

        ColourBlendOp = RhiBlendOperation.Add;
        AlphaBlendOp = RhiBlendOperation.Add;

        DestinationColourFactor = RhiBlendFactor.Zero;
        DestinationAlphaFactor = RhiBlendFactor.Zero;

        WriteMask = RhiColourMask.RGBA;

        Name = "";
    }

    private RhiBlendState(string name, 
                          RhiBlendFactor sourceColourFactor, RhiBlendFactor sourceAlphaFactor, 
                          RhiBlendFactor destinationColourFactor, RhiBlendFactor destinationAlphaFactor) : this()
    {
        Name = name;

        SourceColourFactor = sourceColourFactor;
        SourceAlphaFactor = sourceAlphaFactor;

        DestinationColourFactor = destinationColourFactor;
        DestinationAlphaFactor = destinationAlphaFactor;
    }
}
