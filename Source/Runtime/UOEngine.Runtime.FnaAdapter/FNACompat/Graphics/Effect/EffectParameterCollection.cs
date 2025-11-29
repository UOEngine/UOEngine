using System.Collections;
using System.Diagnostics;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.RHI.Resources;

namespace Microsoft.Xna.Framework.Graphics;

public sealed class EffectParameterCollection : IEnumerable<EffectParameter>
{
    private readonly List<EffectParameter> _parameters;
    private readonly Dictionary<string, EffectParameter> _lookup;

    public EffectParameterCollection(List<EffectParameter> parameters)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        _parameters = parameters;
        _lookup = new Dictionary<string, EffectParameter>(StringComparer.OrdinalIgnoreCase);

        foreach (var param in parameters)
        {
            // Avoid duplicate keys (structs can have nested members with same name)
            if (!_lookup.ContainsKey(param.Name))
                _lookup[param.Name] = param;
        }
    }

    public EffectParameter this[int index] => _parameters[index];

    public EffectParameter this[string name]
    {
        get
        {
            if (!_lookup.TryGetValue(name, out var value))
            {
                throw new KeyNotFoundException($"EffectParameter '{name}' not found.");
            }

            return value;
        }
    }

    public int Count => _parameters.Count;

    public IEnumerator<EffectParameter> GetEnumerator() => _parameters.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
