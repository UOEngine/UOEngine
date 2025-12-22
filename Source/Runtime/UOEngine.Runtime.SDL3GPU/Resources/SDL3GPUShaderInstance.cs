using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.SDL3GPU.Resources;

internal class Sdl3GpuShaderResource: RhiShaderResource
{
    public SDL3GPUShaderProgram VertexProgram { get; private set; } = null!;
    public SDL3GPUShaderProgram PixelProgram { get; private set; } = null!;

    private readonly Sdl3GpuDevice _device;

    public Sdl3GpuShaderResource(Sdl3GpuDevice device, in RhiShaderResourceCreateParameters createParameters = default)
    {
        _device = device;
        Name = createParameters.Name;
    }

    public override void Load(string vertexShader, string fragmentShader)
    {
        UOEngineDxcCompiler.Compile(vertexShader, ShaderProgramType.Vertex, out var vertexCompileResult, _device.ShaderFormat, "main");
        UOEngineDxcCompiler.Compile(fragmentShader, ShaderProgramType.Pixel, out var pixelCompileResult, _device.ShaderFormat, "main");

        LoadCommon(vertexCompileResult, pixelCompileResult);
    }

    public override void Load(string shaderFile, string vertexMainName, string pixelNameMain)
    {
        UOEngineDxcCompiler.Compile(shaderFile, ShaderProgramType.Vertex, out var vertexCompileResult, _device.ShaderFormat,vertexMainName);
        UOEngineDxcCompiler.Compile(shaderFile, ShaderProgramType.Pixel, out var pixelCompileResult, _device.ShaderFormat, pixelNameMain);

        LoadCommon(vertexCompileResult, pixelCompileResult);
    }

    private void LoadCommon(in ShaderProgramCompileResult vertexCompileResult, in ShaderProgramCompileResult pixelCompileResult)
    {
        VertexProgram = new SDL3GPUShaderProgram(_device, ShaderProgramType.Vertex, vertexCompileResult);
        PixelProgram = new SDL3GPUShaderProgram(_device, ShaderProgramType.Pixel, pixelCompileResult);

        ProgramBindings[ShaderProgramType.Vertex.ToInt()].Parameters = [.. VertexProgram.InputBindings];
        ProgramBindings[ShaderProgramType.Pixel.ToInt()].Parameters = [.. PixelProgram.InputBindings];
    }
}
