using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

public class VertexBuffer
{
    public readonly IRhiVertexBuffer RhiVertexBuffer;

    public VertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage)
    {

        RhiVertexBuffer = graphicsDevice.RenderResourceFactory.CreateVertexBuffer(new RhiVertexBufferDescription
        {
            VertexCount = (uint)vertexCount,
            Stride = vertexDeclaration.RhiVertexDefinition.Stride,
            AttributesDefinition = vertexDeclaration.RhiVertexDefinition
        });
    }

    public void SetDataPointerEXT(int offsetInBytes, IntPtr data, int dataLength, SetDataOptions options)
    {
        RhiVertexBuffer.SetData(offsetInBytes, data, dataLength);
        RhiVertexBuffer.Upload();
    }

    public void Dispose()
    {

    }
}
