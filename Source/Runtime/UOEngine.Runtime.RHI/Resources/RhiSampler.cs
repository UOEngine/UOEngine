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
    public readonly RhiSamplerFilter Filter { get; init; }

    //public RhiSampler(SamplerFilter filter)
    //{
    //    Filter = filter;
    //}
}
