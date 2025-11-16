using System.Diagnostics;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.RHI.Resources;

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

            Debug.Assert(false);

            return null;
        }
    }

    private List<EffectParameter> elements = [];

    public EffectParameterCollection(byte[] byteCode)
    {
        var techniques = Remapper.GetTechniques(byteCode);

        foreach(var entry in techniques)
        {
            var parameters = entry.Value.GetParameterNames();

            foreach(var param in parameters)
            {
                if(elements.Any(p => p.Name == param) == false)
                {
                    elements.Add(new EffectParameter(param));
                }
            }
        }
    }
}
