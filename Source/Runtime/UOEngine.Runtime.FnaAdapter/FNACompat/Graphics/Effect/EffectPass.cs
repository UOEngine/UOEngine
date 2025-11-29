namespace Microsoft.Xna.Framework.Graphics;

public class EffectPass
{
    internal readonly Effect Parent;
    internal readonly IntPtr Pipeline; // SDL_GPU_Pipeline*
    internal readonly int UniformSlot; // binding slot for uniforms

    public string Name { get; }

    internal EffectPass(Effect effect, string name, IntPtr pipeline, int uniformSlot)
    {
        Parent = effect;
        Name = name;
        Pipeline = pipeline;
        UniformSlot = uniformSlot;
    }

    public void Apply()
    {
        throw new NotImplementedException();
        //Parent.GraphicsDevice.ApplyEffectPass(this);
    }
}
