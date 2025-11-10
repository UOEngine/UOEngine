using System.Collections;

namespace Microsoft.Xna.Framework.Graphics;
public class EffectPassCollection : IEnumerable<EffectPass>, IEnumerable
{
    IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator<EffectPass> System.Collections.Generic.IEnumerable<EffectPass>.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}
