// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;

using Vortice.Dxc;
using Vortice.SPIRV.Reflect;
using static Vortice.SPIRV.Reflect.SPIRVReflectApi;

using UOEngine.Runtime.RHI;
using UOEngine.Runtime.Core;

namespace UOEngine.Runtime.Vulkan;

struct ShaderProgramCompileResult
{
    public byte[] ByteCode;
    public string EntryPointName;
    public ShaderStreamBinding[] StreamBindings;
    public ShaderParameter[] ShaderBindings;
}

internal class UOEngineDxcCompiler
{
    public static void Compile(string shaderFile, ShaderProgramType type, out ShaderProgramCompileResult outCompileResult, string programMainName = "main")
    {
        if(File.Exists(shaderFile) == false)
        {
            throw new FileNotFoundException($"Could not find file {shaderFile}");
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
            "-Od"                                   // Optimization level
        ];

        string[] arguments = strings;

        // -fvk-auto-shift-bindings can auto shift the bindings for spirv slots.
        arguments = [.. arguments, "-spirv", "-fspv-debug=vulkan-with-source"];

        using IDxcResult result = DxcCompiler.Compile(source, arguments);

        if (result.GetStatus().Failure)
        {
            throw new Exception($"Compilation failed for {shaderFile}:\n" + result.GetErrors());
        }

        var blob = result.GetResult();

        outCompileResult = new ShaderProgramCompileResult();
        outCompileResult.EntryPointName = programMainName;

        outCompileResult.ByteCode = blob.AsBytes();

        ReflectSpirv(outCompileResult.ByteCode, out outCompileResult.StreamBindings, out outCompileResult.ShaderBindings);
    }

    private static unsafe void ReflectSpirv(byte[] inByteCode, out ShaderStreamBinding[] outStreamBindings, out ShaderParameter[] outShaderBindings)
    {
        SpvReflectShaderModule module;

        if (spvReflectCreateShaderModule(inByteCode, &module) != SpvReflectResult.Success)
        {
            UOEDebug.NotImplemented();
        }

        List<ShaderParameter> shaderParameters = [];

        for (int i = 0; i < module.descriptor_binding_count; i++)
        {
            var resourceDescription = module.descriptor_bindings[i];

            if(resourceDescription.accessed == 0)
            {
                continue;
            }

            uint size = 0;
            RhiShaderInputType inputType = RhiShaderInputType.Invalid;

            ShaderVariable[] shaderVariables = [];

            if (resourceDescription.descriptor_type == SpvReflectDescriptorType.UniformBuffer)
            {
                SpvReflectBlockVariable constantBuffer = resourceDescription.block;

                inputType = RhiShaderInputType.Constant;

                shaderVariables = new ShaderVariable[constantBuffer.member_count];

                for (uint j = 0; j < constantBuffer.member_count; j++)
                {
                    var variable = constantBuffer.members[j];

                    size += variable.size;

                    RhiShaderVariableType variableType = RhiShaderVariableType.Invalid;

                    SpvReflectTypeFlags typeFlags = variable.type_description[0].type_flags;

                    if(typeFlags.HasFlag(SpvReflectTypeFlags.FlagMatrix))
                    {
                        variableType = RhiShaderVariableType.Matrix;
                    }
                    else if (typeFlags.HasFlag(SpvReflectTypeFlags.FlagVector))
                    {
                        variableType = RhiShaderVariableType.Vector;
                    }
                    else if (typeFlags.HasFlag(SpvReflectTypeFlags.FlagStruct))
                    {
                        variableType = RhiShaderVariableType.Struct;
                    }
                    else
                    {
                        UOEDebug.NotImplemented("");
                    }

                    shaderVariables[j] = new ShaderVariable
                    {
                        Name = variable.Name,
                        Offset = (int)variable.offset,
                        Size = variable.size,
                        Type = variableType
                    };
                }
            }
            else if (resourceDescription.descriptor_type == SpvReflectDescriptorType.Sampler)
            {
                inputType = RhiShaderInputType.Sampler;
            }
            else if (resourceDescription.descriptor_type == SpvReflectDescriptorType.SampledImage)
            {
                inputType = RhiShaderInputType.Texture;
            }
            else
            {
                UOEDebug.NotImplemented();
            }

            shaderParameters.Add(new ShaderParameter
            {
                Name = resourceDescription.Name,
                SlotIndex = resourceDescription.binding,
                Space = resourceDescription.set,
                Size = size,
                InputType = inputType,
                Variables = shaderVariables
            });
        }

        outShaderBindings = shaderParameters.ToArray();


        var streamBindings = new ShaderStreamBinding[(int)module.input_variable_count];

        int inputVariableCount = 0;

        for (int i = 0; i < module.input_variable_count; i++)
        {
            ref var inputVariable = ref module.input_variables[0][i];

            if(inputVariable.built_in != -1)
            {
                // Ignore built in variables
                continue;
            }

            streamBindings[inputVariableCount++] = new ShaderStreamBinding
            {
                SemanticName = inputVariable.Name,
                SemanticIndex = inputVariable.location,
                Format = ToRhiVertexFormat(inputVariable.format)
            };
        }

        outStreamBindings = new ShaderStreamBinding[inputVariableCount];

        if(inputVariableCount > 0)
        {
            Array.Copy(streamBindings, outStreamBindings, inputVariableCount);
        }
    }

    private static RhiVertexAttributeFormat ToRhiVertexFormat(SpvReflectFormat format) => format switch
    {
        SpvReflectFormat.R32Sfloat          => RhiVertexAttributeFormat.Float,
        SpvReflectFormat.R32g32Sfloat       => RhiVertexAttributeFormat.Vector2,
        SpvReflectFormat.R32g32b32Sfloat    => RhiVertexAttributeFormat.Vector3,
        SpvReflectFormat.R32g32b32a32Sfloat => RhiVertexAttributeFormat.Vector4,
        SpvReflectFormat.R32Uint            => RhiVertexAttributeFormat.UInt32,
                                           _ => throw new NotSupportedException()
    };
}
