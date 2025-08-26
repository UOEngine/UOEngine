using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

    public ShaderInstance()
    {
    }

    public void SetTexture(string name, Texture2D texture)
    {
        ShaderInstanceNative.SetTexture(NativeHandle, name, texture.NativeHandle);
    }

    public void SetBuffer(string name, RenderBuffer buffer) 
    {
        ShaderInstanceNative.SetBuffer(NativeHandle, name, buffer.NativeHandle);
    }

    //public unsafe void SetVariable(string name, Matrix4x4 matrix)
    //{
    //    fixed(float* ptr = matrix.AsSpan())
    //    {
    //        ShaderInstanceNative.SetMatrix(NativeHandle, name, (UIntPtr)ptr);
    //    }
    //}

    public void SetVariable<T>(string name, ref T value) where T: unmanaged
    {
        uint size = (uint)Marshal.SizeOf<T>();

        unsafe
        {
            fixed (T* data = &value)
            {
                ShaderInstanceNative.SetVariable(NativeHandle, name, (UIntPtr)data, size);
            }
        }
    }
}
