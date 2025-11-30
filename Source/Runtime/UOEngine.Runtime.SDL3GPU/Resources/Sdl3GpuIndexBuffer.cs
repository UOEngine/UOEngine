using System.Runtime.InteropServices;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.SDL3GPU;

internal class Sdl3GpuIndexBuffer: Sdl3GpuBuffer, IRhiIndexBuffer
{
    public Sdl3GpuIndexBuffer(Sdl3GpuDevice device, uint length, string? name = "")
        : base(device, RhiBufferType.Index, length * sizeof(ushort), name)
    {
        // 16 bit only so far!
    }

    public void SetData(int offsetInBytes, nint data, int byteLength)
    {
        throw new NotImplementedException();
    }

    public void SetData(ReadOnlySpan<ushort> data) => data.CopyTo(MemoryMarshal.Cast<byte, ushort>(Data));
}
