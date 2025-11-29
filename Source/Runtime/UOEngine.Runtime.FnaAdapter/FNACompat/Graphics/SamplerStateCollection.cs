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

    internal SamplerStateCollection(int slots, bool[] modSamplers)
    {
        samplers = new SamplerState[slots];

        modifiedSamplers = modSamplers;

        for (int i = 0; i < samplers.Length; i += 1)
        {
            //samplers[i] = SamplerState.LinearWrap;
        }
    }
}
