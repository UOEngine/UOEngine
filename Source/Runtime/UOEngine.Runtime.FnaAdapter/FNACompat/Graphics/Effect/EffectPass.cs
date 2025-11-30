namespace Microsoft.Xna.Framework.Graphics;

public class EffectPass
{
    public string Name { get; }

    public readonly Effect ParentEffect;
    public readonly int TechniqueIndex;
    public readonly int PassIndex;

    internal EffectPass(string name, Effect parent, int techniqueIndex, int passIndex)
    {
        Name = name;
        ParentEffect = parent;
        TechniqueIndex = techniqueIndex;
        PassIndex = passIndex;
    }

    public void Apply()
    {
        ParentEffect.OnApply();
        ParentEffect.INTERNAL_applyEffect(this);
    }
}
