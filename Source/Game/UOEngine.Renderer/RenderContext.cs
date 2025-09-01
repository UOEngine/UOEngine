using System.Diagnostics;
using UOEngine.Core;
using UOEngine.Interop;

namespace UOEngine.Renderer;

public static class RenderContext
{
    //public static void SetShaderBindingData(Texture2D texture)
    //{
    //    RenderContextNative.SetShaderBindingData(texture.NativeHandle);
    //}

    public static void SetShaderBindingData<T>(ref T data) where T: struct
    {
        //RenderContextNative.SetShaderBindingData(texture.NativeHandle);
    }

    public static ShaderInstance GetShaderInstance()
    {
        UIntPtr shaderHandle = RenderContextNative.GetShaderInstance();

        return new ShaderInstance(shaderHandle);
    }

    public static void SetShaderInstance(ShaderInstance shaderInstance) 
    { 
        //RenderContextNative.SetShaderInstance(shaderInstance.NativeHandle);
    }

    public static unsafe void SetBindlessTextures(ReadOnlySpan<Texture2D> textures, uint? numTextures = null)
    {
        uint length = numTextures ?? (uint)textures.Length;

        Span<UIntPtr> handles = stackalloc UIntPtr[(int)length];

        for(int i = 0; i < length; i++)
        {
            Debug.Assert(textures[i] != null);

            handles[i] = textures[i].NativeHandle;
        }



        fixed(UIntPtr* start = handles)
        {
            RenderContextNative.SetBindlessTextures((nuint)start, length);
        }
    }

    public static void Draw(uint numInstances = 1)
    {
        RenderContextNative.Draw(numInstances);
    }


}
