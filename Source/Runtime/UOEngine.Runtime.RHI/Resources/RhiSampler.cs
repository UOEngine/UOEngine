namespace UOEngine.Runtime.RHI;

public enum SamplerFilter: byte
{
    Invalid,
    Point,
    BilinearNoMips,
    Bilinear,
}

public readonly struct RhiSampler
{
    public readonly SamplerFilter Filter { get; init; }

    //public RhiSampler(SamplerFilter filter)
    //{
    //    Filter = filter;
    //}
}
