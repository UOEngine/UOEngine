// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
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
    public int Offset;
    public RhiShaderVariableType Type;
}

[DebuggerDisplay("{Name}")]
public readonly struct ShaderParameter
{
    public readonly string Name { get; init; }
    public readonly int StartOffset { get; init; }
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
    public readonly int Offset;
    public readonly ShaderProgramType ProgramType;
    public const ushort InvalidHandle = 0xFF;
    public static readonly ShaderBindingHandle Invalid = new(InvalidHandle, ShaderProgramType.Invalid, int.MaxValue);

    public bool IsValid => Handle != InvalidHandle;

    public ShaderBindingHandle(ushort handle, ShaderProgramType shaderProgramType, int offset)
    {
        Handle = handle;
        ProgramType = shaderProgramType;
        Offset = offset;
    }
}

public struct ShaderProgramBindings
{
    public ShaderParameter[] Parameters;
}

public struct RhiShaderResourceCreateParameters
{
    public string Name;
}

[DebuggerDisplay("ShaderResource, {Name}")]
public abstract class RhiShaderResource
{
    public ShaderProgramBindings[] ProgramBindings { get; protected set; } = new ShaderProgramBindings[ShaderProgramType.Count.ToInt()];

    protected uint[] _numTextures = new uint[(int)ShaderProgramType.Count];
    protected uint[] _numSamplers = new uint[(int)ShaderProgramType.Count];

    public abstract void Load(string vertexShader, string fragmentShader);
    public abstract void Load(string shaderFile, string vertexMainName, string pixelNameMain);

    public string? Name { get; protected set; }

    public ShaderProgramBindings GetShaderProgramBindings(ShaderProgramType type) => ProgramBindings[(int)type];

    public ShaderBindingHandle GetBindingHandle(ShaderProgramType programType, RhiShaderInputType inputType, string name)
    {
        for (int i = 0; i < ProgramBindings[(int)programType].Parameters.Length; i++)
        {
            ref var param = ref ProgramBindings[(int)programType].Parameters[i];

            if (param.Name == name && param.InputType == inputType)
            {
                return new ShaderBindingHandle((ushort)i, programType, param.StartOffset);
            }
        }

        var parameterNames = GetParameterNames(programType);

        throw new UnreachableException($"Could not find shader binding handle in vertex shader for {name}. Available variables are: {string.Join(", ", parameterNames)}");
    }

    public ShaderBindingHandle GetBindingHandle(ShaderProgramType programType, RhiShaderInputType inputType, int index)
    {
        int resourceIndex = 0;

        for (int i = 0; i < ProgramBindings[(int)programType].Parameters.Length; i++)
        {
            ref var param = ref ProgramBindings[(int)programType].Parameters[i];

            if (param.InputType == inputType)
            {
                if(resourceIndex == index)
                {
                    return new ShaderBindingHandle((ushort)i, programType, param.StartOffset);
                }
                else
                {
                    resourceIndex++;
                }
            }
        }

        return ShaderBindingHandle.Invalid;
    }

    public ShaderBindingHandle GetBindingHandle(string name)
    {
        for(int programType = 0; programType < (int)ShaderProgramType.Count; programType++)
        {
            for (int i = 0; i < ProgramBindings[programType].Parameters.Length; i++)
            {
                ref var param = ref ProgramBindings[programType].Parameters[i];

                if(param.Name == name)
                {
                    return new ShaderBindingHandle((ushort)i, (ShaderProgramType)programType, param.StartOffset);
                }

                foreach (var variable in param.Variables)
                {
                    if (variable.Name == name)
                    {
                        return new ShaderBindingHandle((ushort)i, (ShaderProgramType)programType, variable.Offset);
                    }
                }
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
            parameterNames.AddRange(GetParameterNames((ShaderProgramType)programType));
        }

        return [.. parameterNames];
    }

    public string[] GetParameterNames(ShaderProgramType programType)
    {
        var programBindings = GetShaderProgramBindings(programType);

        if (programBindings.Parameters == null)
        {
            return [];
        }
        List<string> parameterNames = [];

        for (int i = 0; i < programBindings.Parameters.Length; i++)
        {
            ref var param = ref programBindings.Parameters[i];

            if (param.InputType == RhiShaderInputType.Texture)
            {
                parameterNames.Add(param.Name);
            }
            else
            {
                foreach (var variable in param.Variables)
                {
                    parameterNames.Add(variable.Name);
                }
            }
        }

        return [.. parameterNames];
    }

    public void GetParameter(string name, out ShaderVariable? shaderVariable, out ShaderParameter? shaderParameter)
    {
        shaderVariable = null;
        shaderParameter = null;

        for (int programType = 0; programType < (int)ShaderProgramType.Count; programType++)
        {
            if (ProgramBindings[programType].Parameters == null)
            {
                continue;
            }

            foreach (var parameter in ProgramBindings[programType].Parameters)
            {
                if(parameter.InputType == RhiShaderInputType.Texture)
                {
                    if(parameter.Name == name)
                    {
                        shaderParameter = parameter;

                        return;
                    }
                }

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
