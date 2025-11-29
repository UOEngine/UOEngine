using System.Xml.Linq;

namespace Microsoft.Xna.Framework.Graphics;

public class SamplerState
{
    public string Name;
    public TextureFilter Filter;
    public TextureAddressMode AddressU;
    public TextureAddressMode AddressV;
    public TextureAddressMode AddressW;
    public float mipMapLevelOfDetailBias;
    public int maxAnisotropy;
    public int maxMipLevel;

    public static readonly SamplerState PointClamp;
    public static readonly SamplerState LinearClamp = new("SamplerState.LinearClamp", TextureFilter.Linear, TextureAddressMode.Clamp,
                                                           TextureAddressMode.Clamp, TextureAddressMode.Clamp);

    private SamplerState( string name, TextureFilter filter, TextureAddressMode addressU, TextureAddressMode addressV, TextureAddressMode addressW)
    {
        Name = name;
        Filter = filter;
        AddressU = addressU;
        AddressV = addressV;
        AddressW = addressW;
    }
}