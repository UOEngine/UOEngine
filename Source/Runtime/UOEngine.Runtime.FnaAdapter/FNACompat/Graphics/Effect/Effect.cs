using System.Runtime.CompilerServices;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

internal sealed class EffectData
{
    public readonly byte[] ConstantBuffer;

    public EffectData(uint size)
    {
        ConstantBuffer = new byte[size];
    }
}

public class Effect
{
    internal readonly GraphicsDevice GraphicsDevice;

    internal readonly EffectData Data;

    public EffectParameterCollection Parameters { get; private set; }
    public EffectTechniqueCollection Techniques { get; private set; }

    public EffectTechnique CurrentTechnique { get; set; }

    private readonly UOEEffect _uoeEffectData; 

    public Effect(GraphicsDevice graphicsDevice, byte[] effectCode)
    {
        GraphicsDevice = graphicsDevice;

        GraphicsDevice.EffectRemapper.GetEffect(effectCode, out _uoeEffectData);

        Parse(_uoeEffectData, out Data);

        CurrentTechnique = Techniques[0];
    }

    public Effect(GraphicsDevice graphicsDevice, in UOEEffect uoeEffect)
    {
        GraphicsDevice = graphicsDevice;

        _uoeEffectData = uoeEffect;

        Parse(uoeEffect, out Data);

        CurrentTechnique = Techniques[0];
    }

    public virtual Effect Clone()
    {
        throw new NotImplementedException();
    }

    internal void INTERNAL_applyEffect(EffectPass pass)
    {
        // The pass tells us which variables to use and set.
        //var pass = CurrentTechnique.Passes[(int)passIndex];
        //_uoeEffectData.Techniques[0].Passes[0].

        ref var technique = ref _uoeEffectData.Techniques[pass.TechniqueIndex];

        var shaderInstance = technique.Passes[pass.PassIndex];

        foreach(var parameter in Parameters)
        {
            var bindingHandle = shaderInstance.GetBindingHandle(parameter.Name);

            switch(parameter.Info.Type)
            {
                case RhiShaderVariableType.Matrix:
                {
                    shaderInstance.SetData(bindingHandle, parameter.Data);
                    break;
                }

                default: throw new NotImplementedException();
            };

        }

        GraphicsDevice.ShaderInstance = technique.Passes[pass.PassIndex];
        // Todo: Note effect may have some other graphics pipeline state that needs binding.
    }

    protected Effect(Effect effect)
    {
        throw new NotImplementedException();
    }

    protected internal virtual void OnApply()
    {

    }

    private void Parse(in UOEEffect effect, out EffectData effectData)
    {
        List<EffectParameter> parameters = [];
        List<EffectTechnique> techniques = new List<EffectTechnique>(effect.Techniques.Length);

        uint bufferSizeRequired = 0;

        foreach (var technique in effect.Techniques)
        {
            List<EffectPass> passes = [];

            //techniques.Add(new EffectTechnique(technique.Name, passes));

            foreach (var pass in technique.Passes)
            {
                var parameterNames = pass.GetParameterNames();

                foreach (var parameterName in parameterNames)
                {
                    if(parameters.Any(p => p.Name == parameterName))
                    {
                        continue;
                    }

                    pass.GetVariable(parameterName, out var shaderVariable);

                    bufferSizeRequired += shaderVariable.Size;

                    switch(shaderVariable.Type)
                    {
                        case RhiShaderVariableType.Matrix: break;
                        case RhiShaderVariableType.Scalar: break;
                        case RhiShaderVariableType.Vector: break;
                        default: throw new SwitchExpressionException("Unhandled shader variable type.");
                    }

                    EffectParameter toAdd = new EffectParameter(this, parameterName, shaderVariable);

                    parameters.Add(toAdd);
                }

                passes.Add(new EffectPass("", this, techniques.Count, passes.Count));
            }

            techniques.Add(new EffectTechnique(technique.Name, passes));

            //EffectPassCollection passes = new EffectPassCollection(new EffectPass("pass", annotations, this, 0, 0));

        }

        effectData = new EffectData(bufferSizeRequired);

        Parameters = new EffectParameterCollection(parameters);

        Techniques = new EffectTechniqueCollection(techniques);
    }
    
}
