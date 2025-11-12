namespace Microsoft.Xna.Framework.Graphics;

public class Effect
{
    public EffectTechnique CurrentTechnique;
    public EffectParameterCollection Parameters
    {
        get;
        private set;
    }

    public EffectTechniqueCollection Techniques
    {
        get;
        private set;
    }

    public Effect(GraphicsDevice device, byte[] effectCode)
    {
        throw new NotImplementedException();
    }
}
