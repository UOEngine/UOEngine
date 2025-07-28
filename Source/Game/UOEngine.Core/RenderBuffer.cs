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

        NativeHandle = RenderBufferNative.CreateRenderBuffer((int)numElements, (int)stride);
    }

    public unsafe void SetData<T>(ReadOnlySpan<T> data) where T: struct
    {
        ReadOnlySpan<byte> asBytes = MemoryMarshal.AsBytes(data);

        fixed (byte* ptr = asBytes)
        {
            RenderBufferNative.SetData(NativeHandle, (UIntPtr)ptr, asBytes.Length);
        }
    }
}
