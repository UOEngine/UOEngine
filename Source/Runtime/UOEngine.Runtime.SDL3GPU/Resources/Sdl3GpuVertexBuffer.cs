using System.Runtime.InteropServices;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.SDL3GPU;

internal class Sdl3GpuVertexBuffer: Sdl3GpuBuffer, IRhiVertexBuffer
{
    public readonly RhiVertexDefinition VertexDefinition;

    public Sdl3GpuVertexBuffer(Sdl3GpuDevice device, in RhiVertexBufferDescription description)
        :base(device, RhiBufferType.Vertex, description.VertexCount * description.Stride)
    {
        VertexDefinition = description.AttributesDefinition;
    }

    public void SetData(ReadOnlySpan<ushort> data) => data.CopyTo(MemoryMarshal.Cast<byte, ushort>(Data));

    public void CleanUp()
    {
        Dispose();
    }

}
