using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

public class VertexBuffer : IDisposable
{
    public readonly IRhiVertexBuffer RhiVertexBuffer;

    private bool _disposed = false;

    public VertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage)
        : this(graphicsDevice, vertexDeclaration, vertexCount, bufferUsage, false)
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
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected VertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage, bool dynamic)
    {
        RhiVertexBuffer = graphicsDevice.RenderResourceFactory.CreateVertexBuffer(new RhiVertexBufferDescription
        {
            VertexCount = (uint)vertexCount,
            Stride = vertexDeclaration.RhiVertexDefinition.Stride,
            AttributesDefinition = vertexDeclaration.RhiVertexDefinition,
            IsDynamic = dynamic
        });
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        RhiVertexBuffer.CleanUp();

        _disposed = true;
    }
}
