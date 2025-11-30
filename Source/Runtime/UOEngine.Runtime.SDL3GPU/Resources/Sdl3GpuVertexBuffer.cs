using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.SDL3GPU;

internal class Sdl3GpuVertexBuffer: Sdl3GpuBuffer, IRhiVertexBuffer
{
    public Sdl3GpuVertexBuffer(Sdl3GpuDevice device, in RhiVertexBufferDescription description)
        :base(device, RhiBufferType.Vertex, description.VertexCount * description.Stride)
    {

    }

    public void SetData(int offsetInBytes, nint data, int dataLength)
    {
        throw new NotImplementedException();
    }
}
