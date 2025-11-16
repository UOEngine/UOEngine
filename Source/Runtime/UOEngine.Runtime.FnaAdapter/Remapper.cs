using System.IO.Hashing;

using UOEngine.Runtime.RHI;
using UOEngine.Runtime.RHI.Resources;

namespace UOEngine.Runtime.FnaAdapter;

public struct Technique
{
    public string Name;
    public string VertexMain;
    public string PixelMain;
}

public static class Remapper
{
    private static Dictionary<uint, Dictionary<string, ShaderInstance>> _remappedShaders = [];

    private static IRenderResourceFactory _renderResourceFactory;

    static Remapper()
    {
        _renderResourceFactory = FnaAdapterPlugin.Instance.RenderResourceFactory;
    }

    //public static void RemapShader(string originalFxFile, string newVertexProgram, string newPixelProgram)
    //{
    //    byte[] byteCode = File.ReadAllBytes(originalFxFile);
    //    uint hash = XxHash32.HashToUInt32(byteCode);

    //    var mapEffectResource = _renderResourceFactory.NewShaderResource();

    //    mapEffectResource.Load(newVertexProgram, newPixelProgram);

    //    var shaderInstance = _renderResourceFactory.NewShaderInstance(mapEffectResource);

    //    _remappedShaders.Add(hash, shaderInstance);
    //}

    //public static void RemapShader(string originalFxFile, string newShaderFile, string vertexMain, string pixelMain)
    //{
    //    byte[] byteCode = File.ReadAllBytes(originalFxFile);
    //    uint hash = XxHash32.HashToUInt32(byteCode);

    //    var mapEffectResource = _renderResourceFactory.NewShaderResource();

    //    mapEffectResource.Load(newShaderFile, vertexMain, pixelMain);

    //    var shaderInstance = _renderResourceFactory.NewShaderInstance(mapEffectResource);

    //    _remappedShaders.Add(hash, shaderInstance);
    //}

    public static void RemapTechniques(string originalFxFile, string newShaderFile, Technique[] techniques)
    {
        byte[] byteCode = File.ReadAllBytes(originalFxFile);
        uint hash = XxHash32.HashToUInt32(byteCode);

        Dictionary<string, ShaderInstance> remappedTechniques = [];

        foreach (var technique in techniques)
        {
            var shaderResource = _renderResourceFactory.NewShaderResource();

            shaderResource.Load(newShaderFile, technique.VertexMain, technique.PixelMain);

            var shaderInstance = _renderResourceFactory.NewShaderInstance(shaderResource);

            remappedTechniques.Add(technique.Name, shaderInstance);
        }

        _remappedShaders.Add(hash, remappedTechniques);
    }

    public static Dictionary<string, ShaderInstance> GetTechniques(byte[] originalByteCode)
    {
        uint hash = XxHash32.HashToUInt32(originalByteCode);

        return _remappedShaders[hash];
    }
}
