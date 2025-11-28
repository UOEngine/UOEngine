using UOEngine.Runtime.RHI.Resources;

namespace Microsoft.Xna.Framework.Graphics;

public partial class Effect
{
    private void ParseEffects(byte[] effectCode)
    {
        FNA3D.FNA3D_CreateEffect(effectCode, out var effect);

        List<EffectParameter> parameters = [];
        List<EffectTechnique> techniques = new List<EffectTechnique>(effect.Techniques.Length);

        foreach (var technique in effect.Techniques)
        {
            EffectAnnotationCollection annotations = new([]);

            EffectPassCollection passes = new EffectPassCollection(new EffectPass("pass", annotations, this, 0, 0));

            techniques.Add(new EffectTechnique(technique.Name, technique.Index, passes, null));

            foreach (var pass in technique.Passes)
            {
                foreach (var programBindings in pass.BindingData)
                {
                    if (programBindings.Bindings == null)
                    {
                        continue;
                    }

                    foreach (var binding in programBindings.Bindings)
                    {
                        if (binding.InputType == RhiShaderInputType.Sampler)
                        {
                            continue;
                        }

                        //EffectParameter toAdd = new EffectParameter();

                        //parameters.Add(toAdd);
                    }
                }
            }    
        }

        Parameters = new EffectParameterCollection(parameters);

        Techniques = new EffectTechniqueCollection(techniques);
    }
}
