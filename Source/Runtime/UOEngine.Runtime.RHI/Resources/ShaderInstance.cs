using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace UOEngine.Runtime.RHI;

//[StructLayout(LayoutKind.Explicit)]
public struct ShaderBindingData
{
    //[FieldOffset(0)]
    public RhiSampler Sampler;

    //[FieldOffset(0)]
    public readonly byte[] Buffer;

    //[FieldOffset(0)]
    public IRenderTexture Texture;

    public ShaderBindingData(uint bufferLength)
    {
        Buffer = new byte[bufferLength];
    }
}

[DebuggerDisplay("{BindingIndex}, {InputType}")]
public struct ShaderBindingDataEntry
{
    public readonly uint BindingIndex;
    public readonly RhiShaderInputType InputType;
    public ShaderBindingData Data;

    public IRenderTexture Texture => GetTexture();

    public RhiSampler Sampler => GetSampler();

    public ShaderBindingDataEntry(RhiShaderInputType inputType, uint index, uint dataSize)
    {
        InputType = inputType;
        BindingIndex = index;

        if((InputType == RhiShaderInputType.Buffer) || (InputType == RhiShaderInputType.Constant))
        {
            Debug.Assert(dataSize > 0);

            Data = new ShaderBindingData(dataSize);
        }
    }

    public void SetSampler(RhiSampler sampler)
    {
        Debug.Assert(InputType == RhiShaderInputType.Sampler);

        Data.Sampler = sampler;
    }

    public void SetTexture(IRenderTexture texture)
    {
        Debug.Assert(InputType == RhiShaderInputType.Texture);

        Data.Texture = texture;
    }

    public void SetData<T>(T value) where T: struct
    {
        Debug.Assert(InputType is RhiShaderInputType.Buffer or RhiShaderInputType.Constant);

        MemoryMarshal.Write(Data.Buffer.AsSpan(), value);
    }

    public void SetData<T>(in ReadOnlySpan<T> value) where T : struct
    {
        Debug.Assert(InputType is RhiShaderInputType.Buffer or RhiShaderInputType.Constant);

        var dst = Data.Buffer.AsSpan();  
        var src = MemoryMarshal.AsBytes(value);       

        if (src.Length > dst.Length)
        {
            throw new ArgumentException("Destination too small", nameof(value));
        }

        src.CopyTo(dst);
    }

    public IRenderTexture GetTexture()
    {
        Debug.Assert(InputType == RhiShaderInputType.Texture);

        return Data.Texture;

    }

    public RhiSampler GetSampler()
    {
        Debug.Assert(InputType == RhiShaderInputType.Sampler);

        return Data.Sampler;
    }
}

public struct ShaderProgramBindings1
{
    public ShaderBindingDataEntry[] Bindings;
}

public class ShaderInstance
{
    public readonly ShaderProgramBindings1[] BindingData = new ShaderProgramBindings1[(int)ShaderProgramType.Count];

    public RhiShaderResource ShaderResource => _shaderResource;

    private readonly RhiShaderResource _shaderResource;

    public ShaderInstance(RhiShaderResource shaderResource)
    {
        _shaderResource = shaderResource;

        for (int i = 0; i < _shaderResource.ProgramBindings.Length; i++)
        {
            ref var bindingsForProgram = ref _shaderResource.ProgramBindings[i];

            if(bindingsForProgram.Parameters == null)
            {
                continue;
            }

            BindingData[i].Bindings = new ShaderBindingDataEntry[bindingsForProgram.Parameters.Length];

            ref var bindings = ref BindingData[i].Bindings;
           
            for(int bindEntryIndex = 0; bindEntryIndex < bindings.Length; bindEntryIndex++)
            {
               ref var bindInfo = ref bindingsForProgram.Parameters[bindEntryIndex];

                bindings[bindEntryIndex] = new ShaderBindingDataEntry(bindInfo.InputType, bindInfo.SlotIndex, bindInfo.Size);

            }
        }
    }

    public string[] GetParameterNames()
    {
        return _shaderResource.GetParameterNames();
    }

    public uint GetNumTextures(ShaderProgramType programType)
    {
        return _shaderResource.GetNumTextures(programType);
    }

    public uint GetNumSamplers(ShaderProgramType programType)
    {
        return _shaderResource.GetNumSamplers(programType);
    }

    public ShaderBindingHandle GetBindingHandleConstantVertex(string name)
    {
        return GetBindingHandle(ShaderProgramType.Vertex, RhiShaderInputType.Constant, name);
    }

    public ShaderBindingHandle GetBindingHandleTexturePixel(string name)
    {
        return GetBindingHandle(ShaderProgramType.Pixel, RhiShaderInputType.Texture, name);
    }

    public ShaderBindingHandle GetBindingHandleSamplerPixel(string name)
    {
        return GetBindingHandle(ShaderProgramType.Pixel, RhiShaderInputType.Sampler, name);
    }

    public ShaderBindingHandle GetBindingHandle(ShaderProgramType programType, RhiShaderInputType inputType, string name)
    {
        return _shaderResource.GetBindingHandle(programType, inputType, name);
    }

    public ShaderBindingHandle GetBindingHandle(ShaderProgramType programType, RhiShaderInputType inputType, int index)
    {
        return _shaderResource.GetBindingHandle(programType, inputType, index);
    }

    public ShaderBindingHandle GetBindingHandle(string name)
    {
        return _shaderResource.GetBindingHandle(name);
    }

    public void GetVariable(string name, out ShaderVariable shaderVariable)
    {
        _shaderResource.GetParameter(name, out shaderVariable);
    }

    public void SetParameter(ShaderBindingHandle bindingHandle, in Matrix4x4 matrix)
    {
        ref var entry = ref GetBindingData(bindingHandle);

        entry.SetData(matrix);
    }

    public void SetTexture(ShaderBindingHandle bindingHandle, IRenderTexture texture)
    {
        ref var entry = ref GetBindingData(bindingHandle);

        entry.SetTexture(texture);
    }

    public void SetSampler(ShaderBindingHandle bindingHandle, RhiSampler sampler)
    {
        ref var entry = ref GetBindingData(bindingHandle);

        entry.SetSampler(sampler);
    }

    public void SetData<T>(ShaderBindingHandle bindingHandle, T data) where T : struct
    {
        ref var entry = ref GetBindingData(bindingHandle);

        entry.SetData(data);

    }

    public void SetData<T>(ShaderBindingHandle bindingHandle, ReadOnlySpan<T> data) where T : struct
    {
        ref var entry = ref GetBindingData(bindingHandle);

        entry.SetData(data);
    }

    private ref ShaderBindingDataEntry GetBindingData(ShaderBindingHandle bindingHandle)
    {
        return ref BindingData[(int)bindingHandle.ProgramType].Bindings[bindingHandle.Handle];
    }
}
