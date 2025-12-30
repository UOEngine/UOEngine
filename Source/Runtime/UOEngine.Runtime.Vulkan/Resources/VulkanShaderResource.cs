// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Vulkan;

internal class VulkanShaderResource : RhiShaderResource
{
    internal VulkanShaderProgram VertexProgram => _vertexProgram ?? throw new InvalidOperationException("VulkanShaderResource: Vertex program is not initialised.");
    internal VulkanShaderProgram PixelProgram => _pixelProgram ?? throw new InvalidOperationException("VulkanShaderResource: Vertex program is not initialised.");

    private readonly VulkanDevice _device;
    private readonly string _name;

    private VulkanShaderProgram? _vertexProgram;
    private VulkanShaderProgram? _pixelProgram;

    internal VulkanShaderResource(VulkanDevice device, in RhiShaderResourceCreateParameters parameters)
    {
        _device = device;
        _name = parameters.Name;
    }

    public override void Load(string vertexShader, string fragmentShader)
    {
        UOEngineDxcCompiler.Compile(vertexShader, ShaderProgramType.Vertex, out var vertexCompileResult, true, "main");
        UOEngineDxcCompiler.Compile(fragmentShader, ShaderProgramType.Pixel, out var pixelCompileResult, true, "main");

        LoadCommon(vertexCompileResult, pixelCompileResult);
    }

    public override void Load(string shaderFile, string vertexMainName, string pixelNameMain)
    {
        UOEngineDxcCompiler.Compile(shaderFile, ShaderProgramType.Vertex, out var vertexCompileResult, true, vertexMainName);
        UOEngineDxcCompiler.Compile(shaderFile, ShaderProgramType.Pixel, out var pixelCompileResult,   true, pixelNameMain);

        LoadCommon(vertexCompileResult, pixelCompileResult);
    }

    private void LoadCommon(in ShaderProgramCompileResult vertexCompileResult, in ShaderProgramCompileResult pixelCompileResult)
    {
        _vertexProgram = new VulkanShaderProgram(_device, ShaderProgramType.Vertex, vertexCompileResult);
        _pixelProgram = new VulkanShaderProgram(_device, ShaderProgramType.Pixel, pixelCompileResult);

        ProgramBindings[ShaderProgramType.Vertex.ToInt()].Parameters = [.. VertexProgram.InputBindings];
        ProgramBindings[ShaderProgramType.Pixel.ToInt()].Parameters = [.. PixelProgram.InputBindings];
    }
}
