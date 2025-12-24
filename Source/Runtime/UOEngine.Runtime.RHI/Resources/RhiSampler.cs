// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.RHI;

public enum RhiSamplerFilter: byte
{
    Invalid,
    Point,
    BilinearNoMips,
    Bilinear,
}

public enum RhiTextureAddressMode: byte
{
    Clamp
}

public readonly record struct RhiSampler
{
    public static readonly RhiSampler PointClamp = new("PointClamp", RhiSamplerFilter.Point, RhiTextureAddressMode.Clamp);
    public static readonly RhiSampler Bilinear = new("Bilinear", RhiSamplerFilter.Bilinear, RhiTextureAddressMode.Clamp);

    public readonly RhiSamplerFilter Filter { get; init; }

    public readonly string Name;
    public readonly RhiTextureAddressMode AddressMode;

    public RhiSampler()
    {
        Filter = RhiSamplerFilter.Point;
        AddressMode = RhiTextureAddressMode.Clamp;
        Name = "";
    }

    private RhiSampler(string name, RhiSamplerFilter filter, RhiTextureAddressMode addressMode)
        : this()
    {
        Name = name;
        Filter = filter;
        AddressMode = addressMode;
    }
}
