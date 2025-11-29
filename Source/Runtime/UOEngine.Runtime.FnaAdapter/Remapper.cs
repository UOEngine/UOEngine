using System;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Xna.Framework.Graphics;
using UOEngine.Runtime.RHI;
using UOEngine.Runtime.RHI.Resources;

namespace UOEngine.Runtime.FnaAdapter;

public struct Technique
{
    public string Name;
    public string VertexMain;
    public string PixelMain;
}

public struct UOEEffectTechnique
{
    public string Name;
    public IntPtr Index;
    public ShaderInstance[] Passes;
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

    public void RemapTechniques(string originalFxFile, string newShaderFile, Technique[] techniques)
    {
        byte[] byteCode = File.ReadAllBytes(originalFxFile);
        uint hash = XxHash32.HashToUInt32(byteCode);

        var effectTechniques = new List<UOEEffectTechnique>(techniques.Length);

        int techniqueIndex = 0;

        foreach (var technique in techniques)
        {
            var shaderResource = _renderResourceFactory.NewShaderResource();

            shaderResource.Load(newShaderFile, technique.VertexMain, technique.PixelMain);

            var shaderInstance = _renderResourceFactory.NewShaderInstance(shaderResource);

            effectTechniques.Add(new UOEEffectTechnique
            {
                Name = technique.Name,
                Passes = [shaderInstance],
                Index = techniqueIndex,
            });

            techniqueIndex++;
        }

        _effectsByOriginalByteCodeHash.Add(hash, new UOEEffect
        {
            Techniques = [.. effectTechniques],
        });
    }

    public void RemapEffect<T>(string newShaderFile, Technique[] techniques) where T : Effect
    {
        var effectTechniques = new List<UOEEffectTechnique>(techniques.Length);

        int techniqueIndex = 0;

        foreach (var technique in techniques)
        {
            var shaderResource = _renderResourceFactory.NewShaderResource();

            shaderResource.Load(newShaderFile, technique.VertexMain, technique.PixelMain);

            var shaderInstance = _renderResourceFactory.NewShaderInstance(shaderResource);

            effectTechniques.Add(new UOEEffectTechnique
            {
                Name = technique.Name,
                Passes = [shaderInstance],
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

    //public ShaderInstance GetShaderInstance(IntPtr effect, IntPtr technique)
    //{
    //    return _effects[(int)effect].Techniques[(int)technique].Passes[0];
    //}
}
