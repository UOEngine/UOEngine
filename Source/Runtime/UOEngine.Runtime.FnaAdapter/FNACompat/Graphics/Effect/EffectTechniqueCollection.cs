using System.Collections;
using System.Xml.Linq;

namespace Microsoft.Xna.Framework.Graphics;

public sealed class EffectTechniqueCollection : IEnumerable<EffectTechnique>
{
    private readonly List<EffectTechnique> _techniques;
    private readonly Dictionary<string, EffectTechnique> _lookup;

    public EffectTechniqueCollection(List<EffectTechnique> techniques)
    {
        if (techniques == null)
            throw new ArgumentNullException(nameof(techniques));

        _techniques = techniques;
        _lookup = new Dictionary<string, EffectTechnique>(StringComparer.OrdinalIgnoreCase);

        foreach (var tech in techniques)
        {
            // Support case-insensitive lookup
            if (!_lookup.ContainsKey(tech.Name))
                _lookup[tech.Name] = tech;
        }
    }

    public EffectTechnique this[int index] => _techniques[index];

    public EffectTechnique this[string name]
    {
        get
        {
            if (!_lookup.TryGetValue(name, out var value))
                throw new KeyNotFoundException($"EffectTechnique '{name}' not found.");

            return value;
        }
    }

    public int Count => _techniques.Count;

    public IEnumerator<EffectTechnique> GetEnumerator() => _techniques.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
