using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOEngine.Interop;

namespace UOEngine.Core;

public class ShaderInstance
{
    public readonly UIntPtr NativeHandle;

    public ShaderInstance(UIntPtr nativeHandle)
    {
        NativeHandle = nativeHandle;
    }

    public void SetTexture(string name, Texture2D texture)
    {
        ShaderInstanceNative.SetTexture(NativeHandle, name, texture.NativeHandle);
    }

    public void SetBuffer(string name, RenderBuffer buffer) 
    {
        ShaderInstanceNative.SetBuffer(NativeHandle, name, buffer.NativeHandle);
    }

    public void SetMatrix(string name, Matrix4x4 matrix)
    {
    }

    public void SetInt(string name, int value)
    {
    }
}
