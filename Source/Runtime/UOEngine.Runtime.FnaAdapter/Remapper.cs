using System.IO.Hashing;

using Microsoft.Xna.Framework.Graphics;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.FnaAdapter;

public struct TechniqueProgramEntry
{
    public string VertexMain;
    public string PixelMain;
    public TechniqueProgramEntry(string vertexMain, string pixelMain)
    {
        VertexMain = vertexMain;
        PixelMain = pixelMain;
    }
}

public struct Technique
{
    public string Name;
    public TechniqueProgramEntry[] Programs;
}

public struct UOEEffectPass
{
    public ShaderInstance[] ShaderInstances;
    public string[] VertexParameterNames;
    public string[] PixelParameterNames;
}

public struct UOEEffectTechnique
{
    public string Name;
    public IntPtr Index;
    public UOEEffectPass[] Passes;
}

public struct UOEEffect
{
    public UOEEffectTechnique[] Techniques;
}

public class Remapper
{
    private Dictionary<uint, UOEEffect> _effectsByOriginalByteCodeHash = [];

    private Dictionary<Type, UOEEffect> _effectsByOriginalType = [];

    private readonly IRenderResourceFactory _renderResourceFactory;

    public Remapper(IRenderResourceFactory renderResourceFactory)
    {
        _renderResourceFactory = renderResourceFactory;
    }

    public void RemapTechniques(string originalFxFile, string newShaderFile, Technique[] techniques, string effectName)
    {
        byte[] byteCode = File.ReadAllBytes(originalFxFile);
        uint hash = XxHash32.HashToUInt32(byteCode);

        var effectTechniques = new List<UOEEffectTechnique>(techniques.Length);

        int techniqueIndex = 0;

        foreach (var technique in techniques)
        {
            ShaderInstance[] shaderInstances = new ShaderInstance[technique.Programs.Length];

            for (int i = 0; i < technique.Programs.Length; i++)
            {
                var shaderResource = _renderResourceFactory.NewShaderResource(new RhiShaderResourceCreateParameters
                {
                    Name = $"{effectName}.{technique.Name}"
                });

                ref var programSet = ref technique.Programs[i];

                shaderResource.Load(newShaderFile, programSet.VertexMain, programSet.PixelMain);

                shaderInstances[i] = _renderResourceFactory.NewShaderInstance(shaderResource);
            }


            effectTechniques.Add(new UOEEffectTechnique
            {
                Name = technique.Name,
                Passes = [new UOEEffectPass { ShaderInstances = shaderInstances}],
                Index = techniqueIndex,
            });

            techniqueIndex++;
        }

        _effectsByOriginalByteCodeHash.Add(hash, new UOEEffect
        {
            Techniques = [.. effectTechniques],
        });
    }

    public void RemapEffect<T>(string newShaderFile, Technique[] techniques, string name) where T : Effect
    {
        var effectTechniques = new List<UOEEffectTechnique>(techniques.Length);

        int techniqueIndex = 0;

        foreach (var technique in techniques)
        {
            UOEEffectPass[] passes = new UOEEffectPass[technique.Programs.Length];

            for (int i = 0; i < technique.Programs.Length; i++)
            {
                var shaderResource = _renderResourceFactory.NewShaderResource(new RhiShaderResourceCreateParameters
                {
                    Name = name
                });

                ref var programSet = ref technique.Programs[i];

                shaderResource.Load(newShaderFile, programSet.VertexMain, programSet.PixelMain);

                var shaderInstance = _renderResourceFactory.NewShaderInstance(shaderResource);

                passes[i].ShaderInstances = [shaderInstance];
                passes[i].VertexParameterNames = shaderInstance.GetParameterNames(ShaderProgramType.Vertex);
                passes[i].PixelParameterNames = shaderInstance.GetParameterNames(ShaderProgramType.Pixel);
            }

            effectTechniques.Add(new UOEEffectTechnique
            {
                Name = technique.Name,
                Passes = passes,
                Index = techniqueIndex,
            });

            techniqueIndex++;
        }

        _effectsByOriginalType.Add(typeof(T), new UOEEffect
        {
            Techniques = [.. effectTechniques],
        });
    }

    public void GetEffect(byte[] originalByteCode, out UOEEffect effect)
    {
        uint hash = XxHash32.HashToUInt32(originalByteCode);

        effect = _effectsByOriginalByteCodeHash[hash];
    }

    public UOEEffect GetEffect<T>() where T: Effect => _effectsByOriginalType[typeof(T)];

}
