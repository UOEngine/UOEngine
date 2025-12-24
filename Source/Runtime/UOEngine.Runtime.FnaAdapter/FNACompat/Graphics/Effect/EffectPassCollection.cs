using System.Collections;

namespace Microsoft.Xna.Framework.Graphics;
public class EffectPassCollection : IEnumerable<EffectPass>, IEnumerable
{
    public EffectPass this[int index] => _passes[index];

    public EffectPass this[string name] => _lookup[name];

    public int Count => _passes.Count;
    private readonly List<EffectPass> _passes;
    private readonly Dictionary<string, EffectPass> _lookup;

    public EffectPassCollection(List<EffectPass> passes)
    {
        _passes = passes ?? throw new ArgumentNullException(nameof(passes));
        _lookup = new Dictionary<string, EffectPass>(StringComparer.OrdinalIgnoreCase);

        foreach (var pass in passes)
        {
            _lookup[pass.Name] = pass;
        }
    }
    public IEnumerator<EffectPass> GetEnumerator() => _passes.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
