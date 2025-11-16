using System.Security.Cryptography;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.RHI.Resources;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Microsoft.Xna.Framework.Graphics;

public class Effect
{
    public EffectTechnique CurrentTechnique;

    private readonly GraphicsDevice _device;
    private readonly ShaderInstance _shaderInstance;

    public EffectParameterCollection Parameters
    {
        get;
        private set;
    }

    public EffectTechniqueCollection Techniques
    {
        get;
        private set;
    }

    public Effect(GraphicsDevice device, byte[] effectCode)
    {
        _device = device;

        Parameters = new EffectParameterCollection(effectCode);
    }
}
