using System.Xml.Linq;

namespace Microsoft.Xna.Framework.Graphics;

public class EffectTechniqueCollection
{
    public EffectTechnique this[string name]
    {
        get
        {
            foreach (EffectTechnique elem in elements)
            {
                if (name.Equals(elem.Name))
                {
                    return elem;
                }
            }
            return null;
        }
    }

    private List<EffectTechnique> elements;

}
