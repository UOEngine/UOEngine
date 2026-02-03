using System.Runtime.InteropServices;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

public class IndexBuffer
{
    public IRhiBuffer RhiIndexBuffer { get; private set; } = null!;

    private readonly uint _size;

    public IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage bufferUsage) 
    {
        _size = (uint)(indexCount * sizeof(ushort));

        RhiIndexBuffer = graphicsDevice.RenderResourceFactory.NewBuffer(new RhiBufferDescription
        {
            Size = _size,
            Usage = RhiBufferUsageFlags.Dynamic | RhiBufferUsageFlags.Index,
            Name = "FnaIndexBuffer"
        });
    }

    public unsafe void SetDataPointerEXT(int offsetInBytes, IntPtr data, int dataLength, SetDataOptions options)
    {
        Span<byte> indices = RhiIndexBuffer.Lock((uint)dataLength, (uint)offsetInBytes);

        var src = new ReadOnlySpan<byte>((void*)data, dataLength);

        src.CopyTo(indices);

        RhiIndexBuffer.Unlock();
    }

    public void SetData(short[] data)
    {
        Span<byte> indices = RhiIndexBuffer.Lock(_size, 0);

        MemoryMarshal.AsBytes(data.AsSpan()).CopyTo(indices);

        RhiIndexBuffer.Unlock();
    }

    public void Dispose()
    {

    }
}
