using Microsoft.Xna.Framework.Graphics;

namespace UOEngine.Runtime.Renderer.Resources;

public class ShaderInstance
{
    public readonly Effect _effect;

    public ShaderInstance(GraphicsDevice graphicsDevice, byte[] shaderBytecode)
    {
        _effect = new Effect(graphicsDevice, shaderBytecode);
    }

    public void Bind()
    {
        _effect.CurrentTechnique.Passes[0].Apply();
    }

    public void SetParameter(string name, object value)
    {
        var parameter = _effect.Parameters[name];

        switch (value)
        {
            case float f: parameter.SetValue(f); break;

            default:
                throw new ArgumentException($"Unsupported type '{value.GetType()}' for parameter '{name}'");
        }
    }
}
