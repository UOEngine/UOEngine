using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UOEngine.Interop;

namespace UOEngine;

public class RenderBuffer
{
    public readonly uint Stride;
    public readonly uint NumElements;

    public readonly UIntPtr NativeHandle;

    public RenderBuffer(uint numElements, uint stride)
    {
        NumElements = numElements;
        Stride = stride;

        NativeHandle = RenderBufferNative.CreateRenderBuffer(numElements, stride);
    }

    public unsafe void SetData<T>(ReadOnlySpan<T> data) where T: struct
    {
        ReadOnlySpan<byte> asBytes = MemoryMarshal.AsBytes(data);

        fixed (byte* ptr = asBytes)
        {
            RenderBufferNative.SetData(NativeHandle, (UIntPtr)ptr, (uint)asBytes.Length);
        }
    }

    public unsafe void SetData<T>(T value) where T: struct
    {
        ReadOnlySpan<T> span  = MemoryMarshal.CreateReadOnlySpan(ref value, 1);

        SetData(span);
    }
}
