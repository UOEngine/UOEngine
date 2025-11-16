using System.Runtime.InteropServices;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

public class IndexBuffer
{
    private IRhiIndexBuffer _rhiIndexBuffer;

    public IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage bufferUsage) 
    {
        _rhiIndexBuffer = FnaAdapterPlugin.Instance.RenderResourceFactory.CreateIndexBuffer((uint)indexCount, "FnaIndexBuffer");
    }

    public void SetDataPointerEXT(int offsetInBytes, IntPtr data, int dataLength, SetDataOptions options)
    {
        throw new NotImplementedException();
    }

    public void SetData(short[] data)
    {
        _rhiIndexBuffer.SetData(MemoryMarshal.Cast<short, ushort>(data));
        _rhiIndexBuffer.Upload();
    }

    public void Dispose()
    {

    }
}
