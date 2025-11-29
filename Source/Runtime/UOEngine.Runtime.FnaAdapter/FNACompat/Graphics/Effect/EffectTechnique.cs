namespace Microsoft.Xna.Framework.Graphics;

public class EffectTechnique
{
    public string Name { get; }

    public EffectPassCollection Passes { get; }

    internal EffectTechnique(string name, List<EffectPass> passes)
    {
        Name = name;
        Passes = new EffectPassCollection(passes);
    }
}
