using System.Runtime.InteropServices;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

public class IndexBuffer
{
    public IRhiIndexBuffer RhiIndexBuffer { get; private set; } = null!;

    public IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage bufferUsage) 
    {
        UOEDebug.NotImplemented();

        //RhiIndexBuffer = graphicsDevice.RenderResourceFactory.NewBuffer(new RhiBufferDescription
        //{
        //    Size = (uint)(indexCount * sizeof(ushort))
        //});// (uint)indexCount, "FnaIndexBuffer");
    }

    public void SetDataPointerEXT(int offsetInBytes, IntPtr data, int dataLength, SetDataOptions options)
    {
        RhiIndexBuffer.SetData(offsetInBytes, data, dataLength);
        RhiIndexBuffer.Upload();
    }

    public void SetData(short[] data)
    {
        RhiIndexBuffer.SetData(MemoryMarshal.Cast<short, ushort>(data));
        RhiIndexBuffer.Upload();
    }

    public void Dispose()
    {

    }
}
