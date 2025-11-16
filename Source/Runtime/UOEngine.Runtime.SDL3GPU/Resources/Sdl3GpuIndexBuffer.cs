using UOEngine.Runtime.RHI;
using UOEngine.Runtime.RHI.Resources;

namespace UOEngine.Runtime.SDL3GPU.Resources;

internal class Sdl3GpuIndexBuffer: Sdl3GpuBuffer<ushort>, IRhiIndexBuffer
{
    public ushort[] Indices => Data;

    public Sdl3GpuIndexBuffer(Sdl3GpuDevice device, uint length, string? name = "")
        : base(device, RenderBufferType.Index, length, name)
    {

    }

}
