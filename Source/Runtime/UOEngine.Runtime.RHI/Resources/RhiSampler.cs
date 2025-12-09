namespace UOEngine.Runtime.RHI;

public enum RhiSamplerFilter: byte
{
    Invalid,
    Point,
    BilinearNoMips,
    Bilinear,
}

public readonly record struct RhiSampler
{
    public static readonly RhiSampler Point = new(RhiSamplerFilter.Point);
    public static readonly RhiSampler Bilinear = new(RhiSamplerFilter.Bilinear);

    public readonly RhiSamplerFilter Filter { get; init; }

    public RhiSampler(RhiSamplerFilter filter)
    {
        Filter = filter;
    }
}
