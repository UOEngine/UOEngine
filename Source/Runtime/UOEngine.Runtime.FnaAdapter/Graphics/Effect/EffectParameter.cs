using System.Diagnostics;
using UOEngine.Runtime.RHI.Resources;

namespace Microsoft.Xna.Framework.Graphics;

[DebuggerDisplay("{Name}")]
public class EffectParameter
{
    public readonly string Name;

    public EffectParameter(string name)
    {
        Name = name;
    }

    public void SetValue(Vector4 value)
    {
    }

    public void SetValue(Matrix value)
    {
    }

}
