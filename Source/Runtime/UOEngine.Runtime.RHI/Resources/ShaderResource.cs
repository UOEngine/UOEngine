using System.Diagnostics;

namespace UOEngine.Runtime.RHI;

public enum RhiShaderInputType
{
    Buffer,
    Texture,
    Sampler,
    Constant,
    Count,
    Invalid
}

public enum RhiShaderVariableType
{
    Scalar,
    Vector,
    Matrix,
    Struct,
    Object,

    Count,
    Invalid
}

[DebuggerDisplay("{Name}, {Type}")]
public struct ShaderVariable
{
    public string Name;
    public uint Size;
    public uint Offset;
    public RhiShaderVariableType Type;
}

[DebuggerDisplay("{Name}")]
public readonly struct ShaderParameter
{
    public readonly string Name { get; init; }
    public readonly uint StartOffset { get; init; }
    public readonly uint Size { get; init; }
    public readonly RhiShaderInputType InputType { get; init; }
    public readonly uint SlotIndex { get; init; }
    public readonly ShaderVariable[] Variables { get; init; }
}

[DebuggerDisplay("{SemanticName}, {SemanticIndex}")]
public struct ShaderStreamBinding
{
    public string SemanticName;
    public uint SemanticIndex;
    public RhiVertexAttributeFormat Format;
}

public readonly struct ShaderBindingHandle
{
    public readonly ushort Handle;
    public readonly ShaderProgramType ProgramType;
    public const ushort InvalidHandle = 0xFF;
    public static readonly ShaderBindingHandle Invalid = new(InvalidHandle, ShaderProgramType.Invalid);

    public bool IsValid => Handle != InvalidHandle;

    public ShaderBindingHandle(ushort handle, ShaderProgramType shaderProgramType)
    {
        ProgramType = shaderProgramType;
        Handle = handle;
    }
}

public struct ShaderProgramBindings
{
    public ShaderParameter[] Parameters;
}

public abstract class RhiShaderResource
{
    public ShaderProgramBindings[] ProgramBindings { get; protected set; } = new ShaderProgramBindings[ShaderProgramType.Count.ToInt()];

    protected uint[] _numTextures = new uint[(int)ShaderProgramType.Count];
    protected uint[] _numSamplers = new uint[(int)ShaderProgramType.Count];

    public abstract void Load(string vertexShader, string fragmentShader);
    public abstract void Load(string shaderFile, string vertexMainName, string pixelNameMain);


    public ShaderBindingHandle GetBindingHandle(ShaderProgramType programType, RhiShaderInputType inputType, string name)
    {
        for (int i = 0; i < ProgramBindings[(int)programType].Parameters.Length; i++)
        {
            ref var param = ref ProgramBindings[(int)programType].Parameters[i];

            if (param.Name == name && param.InputType == inputType)
            {
                return new ShaderBindingHandle((ushort)i, programType);
            }
        }

        throw new UnreachableException("Could not find shader binding handle in vertex shader.");
    }

    public uint GetNumTextures(ShaderProgramType programType)
    {
        return _numTextures[(int)programType];
    }

    public uint GetNumSamplers(ShaderProgramType programType)
    {
        return _numSamplers[(int)programType];
    }

    public string[] GetParameterNames()
    {
        List<string> parameterNames = [];

        for(int programType = 0; programType < (int)ShaderProgramType.Count; programType++)
        {
            if(ProgramBindings[programType].Parameters == null)
            {
                continue;
            }

            for (int i = 0; i < ProgramBindings[programType].Parameters.Length; i++)
            {
                ref var param = ref ProgramBindings[programType].Parameters[i];

                foreach(var variable in param.Variables)
                {
                    parameterNames.Add(variable.Name);
                }
            }
        }

        return parameterNames.ToArray();
    }

    public void GetParameter(string name, out ShaderVariable shaderVariable)
    {
        for (int programType = 0; programType < (int)ShaderProgramType.Count; programType++)
        {
            if (ProgramBindings[programType].Parameters == null)
            {
                continue;
            }

            foreach (var parameter in ProgramBindings[programType].Parameters)
            {
                foreach(var variable in parameter.Variables)
                {
                    if (variable.Name == name)
                    {
                        shaderVariable = variable;

                        return;
                    }
                }

            }
        }

        throw new InvalidOperationException();
    }
}
