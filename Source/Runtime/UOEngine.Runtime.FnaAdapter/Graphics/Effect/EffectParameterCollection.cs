namespace Microsoft.Xna.Framework.Graphics;

public class EffectParameterCollection
{
    public EffectParameter this[string name]
    {
        get
        {
            foreach (EffectParameter elem in elements)
            {
                if (name.Equals(elem.Name))
                {
                    return elem;
                }
            }
            return null;
        }
    }

    private List<EffectParameter> elements;
}
