using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

public class VertexBuffer : IDisposable
{
    public IRhiBuffer RhiVertexBuffer { get; private set; }

    public readonly VertexDeclaration VertexDeclaration;

    private GraphicsDevice _graphicsDevice;
    private int _vertexCount;
    private bool _dynamic;
    private bool _disposed = false;

    public VertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage)
        : this(graphicsDevice, vertexDeclaration, vertexCount, bufferUsage, false)
    {
    }

    public unsafe void SetDataPointerEXT(int offsetInBytes, IntPtr data, int dataLength, SetDataOptions options)
    {
        Span<byte> vertexData = RhiVertexBuffer.Lock((uint)dataLength, (uint)offsetInBytes);

        var src = new ReadOnlySpan<byte>((void*)data, dataLength);

        src.CopyTo(vertexData);

        RhiVertexBuffer.Unlock();
    }

    protected VertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage, bool dynamic)
    {
        _graphicsDevice = graphicsDevice;
        VertexDeclaration = vertexDeclaration;
        _vertexCount = vertexCount;
        _dynamic = dynamic;

        RhiBufferUsageFlags flags = RhiBufferUsageFlags.Vertex;

        if (_dynamic)
        {
            flags |= RhiBufferUsageFlags.Dynamic;
        }

        RhiVertexBuffer = _graphicsDevice.RenderResourceFactory.NewBuffer(new RhiBufferDescription
        {
            Size = (uint)vertexCount * VertexDeclaration.RhiVertexDefinition.Stride,
            Usage = flags,
            Stride = VertexDeclaration.RhiVertexDefinition.Stride,
        });

        //RhiVertexBuffer = _vertexBuffers[0];
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            //RhiVertexBuffer.Dis
            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~VertexBuffer()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
