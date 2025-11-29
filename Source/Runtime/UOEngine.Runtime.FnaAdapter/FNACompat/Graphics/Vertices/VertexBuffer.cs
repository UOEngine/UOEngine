using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

public class VertexBuffer
{
    private readonly IRhiVertexBuffer _rhiVertexBuffer;

    public VertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage)
    {
        _rhiVertexBuffer = FnaAdapterPlugin.Instance.RenderResourceFactory.CreateVertexBuffer(new RhiVertexBufferDescription
        {
            VertexCount = (uint)vertexCount
        });
    }

    public void SetDataPointerEXT(int offsetInBytes, IntPtr data, int dataLength, SetDataOptions options)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {

    }
}
