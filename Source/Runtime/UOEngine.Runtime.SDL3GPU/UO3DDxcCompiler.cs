// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;
using System.Runtime.CompilerServices;

using static SDL3.SDL;
using Vortice.Direct3D;
using Vortice.Direct3D12.Shader;
using Vortice.Dxc;

using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.SDL3GPU;

struct ShaderProgramCompileResult
{
    public byte[] ByteCode;
    public string EntryPointName;
    public ShaderStreamBinding[] StreamBindings;
    public ShaderParameter[] ShaderBindings;
}

internal class UOEngineDxcCompiler
{
    public static void Compile(string shaderFile, ShaderProgramType type, out ShaderProgramCompileResult outCompileResult, SDL_GPUShaderFormat shaderFormat, string programMainName = "main")
    {
        if(File.Exists(shaderFile) == false)
        {
            throw new Exception($"Could not find file {shaderFile}");
        }

        var source = File.ReadAllText(shaderFile);

        string targetProfileType = "";
        string shaderModelVersion = "6_0";

        switch (type)
        {
            case ShaderProgramType.Vertex: targetProfileType = "vs"; break;
            case ShaderProgramType.Pixel: targetProfileType = "ps"; break;
            default: Debug.Assert(false); break;
        }

        string targetProfile = $"{targetProfileType}_{shaderModelVersion}";

        string[] strings = [
            "-E",               programMainName,
            "-T",               targetProfile,
            "-Zi",                                  // Debug info
            "-Qembed_debug",                        // Embed debug info in the shader
            "-O0"                                   // Optimization level
        ];

        string[] arguments = strings;

        if(shaderFormat == SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV)
        {
            arguments = [.. arguments, "-spirv"];
        }

        using IDxcResult result = DxcCompiler.Compile(source, arguments);

        if (result.GetStatus().Failure)
        {
            throw new Exception("Compilation failed:\n" + result.GetErrors());
        }

        var blob = result.GetResult();

        outCompileResult = new ShaderProgramCompileResult();
        outCompileResult.EntryPointName = programMainName;

        if(shaderFormat != SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_DXIL)
        {
            // Can only get reflection from DXIL. Compile it again as DXIL and attach the SPIRV blob.
            Compile(shaderFile, type, out var resultForReflection, SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_DXIL, programMainName);

            outCompileResult = resultForReflection;
            outCompileResult.ByteCode = blob.AsBytes();

            return;
        }

        outCompileResult.ByteCode = blob.AsBytes();

        using ID3D12ShaderReflection reflection = DxcCompiler.Utils.CreateReflection<ID3D12ShaderReflection>(blob);

        List<ShaderParameter> shaderParameters = [];

        for(uint i = 0; i < reflection.BoundResources.Length; i++)
        {
            var resourceDescription = reflection.BoundResources[i];

            uint size = 0;
            RhiShaderInputType inputType = RhiShaderInputType.Invalid;

            ShaderVariable[] shaderVariables = [];

            if (resourceDescription.Type == ShaderInputType.ConstantBuffer)
            {
                if (resourceDescription.Space != 1)
                {
                    throw new ArgumentException("Constant buffer variables must be in shader register space 1 for Sdl3Gpu");
                }

                ID3D12ShaderReflectionConstantBuffer constantBuffer = reflection.GetConstantBufferByName(resourceDescription.Name);

                inputType = RhiShaderInputType.Constant;

                shaderVariables = new ShaderVariable[constantBuffer.Variables.Length];

                for (uint j = 0; j < constantBuffer.Variables.Length; j++)
                {
                    var variable = constantBuffer.GetVariableByIndex(j);

                    ShaderVariableDescription varDesc = variable.Description;

                    size += varDesc.Size;

                    RhiShaderVariableType variableType = RhiShaderVariableType.Invalid;

                    switch (variable.VariableType.Description.Class)
                    {
                        case ShaderVariableClass.MatrixRows:
                        case ShaderVariableClass.MatrixColumns:
                            {
                                variableType = RhiShaderVariableType.Matrix;
                                break;
                            }

                        case ShaderVariableClass.Scalar:
                            {
                                variableType = RhiShaderVariableType.Scalar;

                                break;
                            }

                        case ShaderVariableClass.Vector:
                            {
                                variableType = RhiShaderVariableType.Vector;

                                break;
                            }

                        case ShaderVariableClass.Object:
                            {
                                variableType = RhiShaderVariableType.Object;

                                break;
                            }

                        case ShaderVariableClass.Struct:
                            {
                                variableType = RhiShaderVariableType.Struct;

                                break;
                            }

                        default:
                            throw new SwitchExpressionException("Unhandled ShaderVariableClass type");
                    }

                    shaderVariables[j] = new ShaderVariable
                    {
                        Name = varDesc.Name,
                        Offset = (int)varDesc.StartOffset,
                        Size = varDesc.Size,
                        Type = variableType
                    };
                }
            }
            else if(resourceDescription.Type == ShaderInputType.Sampler)
            {
                inputType = RhiShaderInputType.Sampler;

                if(resourceDescription.Space != 2)
                {
                    throw new ArgumentException("Samplers must be in shader register space 2 for Sdl3Gpu");
                }
            }
            else if (resourceDescription.Type == ShaderInputType.Texture)
            {
                if (resourceDescription.Space != 2)
                {
                    throw new ArgumentException("Textures must be in shader register space 2 for Sdl3Gpu");
                }

                inputType = RhiShaderInputType.Texture;

                var t = reflection.GetVariableByName(resourceDescription.Name);

            }
            else
            {
                Debug.Assert(false);
            }

            shaderParameters.Add(new ShaderParameter
            {
                Name = resourceDescription.Name,
                SlotIndex = resourceDescription.BindPoint,
                Size = size,
                InputType = inputType,
                Variables = shaderVariables
            });
        }

        outCompileResult.ShaderBindings = [.. shaderParameters];

        List<ShaderStreamBinding> streamBindings = new List<ShaderStreamBinding>(reflection.InputParameters.Length);

        for (int i = 0; i < reflection.InputParameters.Length; i++)
        {
            var param = reflection.InputParameters[i];

            if(param.SystemValueType != SystemValueType.Undefined)
            {
                // SV_InstanceID, SV_VertexID, etc.
                continue;
            }

            // Is vertex attribute.

            streamBindings.Add(new ShaderStreamBinding
            {
                SemanticName = param.SemanticName,
                SemanticIndex = param.Register,
                Format = ToRhiVertexFormat(param.ComponentType, (byte)param.UsageMask)
            });
        }

        outCompileResult.StreamBindings = [.. streamBindings];
    }

    private static RhiVertexAttributeFormat ToRhiVertexFormat(RegisterComponentType type, byte mask) => (type, mask) switch
    {
        (RegisterComponentType.Float32, 0x1) => RhiVertexAttributeFormat.Float,
        (RegisterComponentType.Float32, 0x3) => RhiVertexAttributeFormat.Vector2,
        (RegisterComponentType.Float32, 0x7) => RhiVertexAttributeFormat.Vector3,
        (RegisterComponentType.Float32, 0xF) => RhiVertexAttributeFormat.Vector4,
        (RegisterComponentType.UInt32,  0x1) => RhiVertexAttributeFormat.UInt32,
                                           _ => throw new NotSupportedException()
    };
}
