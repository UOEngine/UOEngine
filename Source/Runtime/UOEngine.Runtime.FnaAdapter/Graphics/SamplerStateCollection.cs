namespace Microsoft.Xna.Framework.Graphics;

public class SamplerStateCollection
{
    public SamplerState this[int index]
    {
        get
        {
            return samplers[index];
        }
        set
        {
            samplers[index] = value;
            modifiedSamplers[index] = true;
        }
    }

    private readonly SamplerState[] samplers;
    private readonly bool[] modifiedSamplers;
}
