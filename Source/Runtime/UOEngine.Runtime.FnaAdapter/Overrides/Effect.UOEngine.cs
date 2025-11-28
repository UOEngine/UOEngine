using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

public partial class Effect
{
    struct EffectData
    {
        public byte[] Value;
        public GCHandle Handle;
        public IntPtr Ptr;

        public EffectData(uint size)
        {
            Value = new byte[size];
            Handle = GCHandle.Alloc(Value, GCHandleType.Pinned);
            Ptr = Handle.AddrOfPinnedObject();
        }
    }

    private List<EffectData> _shaderData = [];

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
                var parameterNames = pass.GetParameterNames();

                foreach(var parameterName in parameterNames)
                {
                    pass.GetVariable(parameterName, out var shaderVariable);

                    EffectParameterClass effectParameterClass;

                    switch (shaderVariable.Type)
                    {
                        case RhiShaderVariableType.Scalar: effectParameterClass = EffectParameterClass.Scalar; break;
                        case RhiShaderVariableType.Vector: effectParameterClass = EffectParameterClass.Vector; break;
                        case RhiShaderVariableType.Matrix: effectParameterClass = EffectParameterClass.Matrix; break;
                        case RhiShaderVariableType.Struct: effectParameterClass = EffectParameterClass.Struct; break;
                        case RhiShaderVariableType.Object: effectParameterClass = EffectParameterClass.Object; break;

                        default: throw new SwitchExpressionException();
                    }
                    EffectParameterType effectParameterType = EffectParameterType.Void;

                    var effectData = new EffectData(shaderVariable.Size);

                    _shaderData.Add(effectData);

                    EffectParameter toAdd = new EffectParameter(parameterName, null, 0, 0, 0, effectParameterClass,
                        effectParameterType, IntPtr.MaxValue, null, effectData.Ptr, shaderVariable.Size, this);

                    parameters.Add(toAdd);
                }
            }    
        }

        Parameters = new EffectParameterCollection(parameters);

        Techniques = new EffectTechniqueCollection(techniques);
    }
}
