using System.Diagnostics;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

public class VertexBuffer : IDisposable
{
    public IRhiVertexBuffer RhiVertexBuffer { get; private set; }

    private bool _disposed = false;

    private List<IRhiVertexBuffer> _vertexBuffers = [];

    private GraphicsDevice _graphicsDevice;
    private VertexDeclaration _vertexDeclaration;
    private int _vertexCount;
    private bool _dynamic;

    private int _activeVertexBufferIndex = 0;
    private int _timesCalledPerFrame = 0;

    public VertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage)
        : this(graphicsDevice, vertexDeclaration, vertexCount, bufferUsage, false)
    {
    }

    public void SetDataPointerEXT(int offsetInBytes, IntPtr data, int dataLength, SetDataOptions options)
    {
        // Doing this is meh. 
        // FNA seems to hold them all in different transfer buffers before flushing them when the limit is reached
        // and also flushing the command lists to submit them and do the draws, even mid frame draw.
        // Not a good idea but this is the easiest way to get this working for now. 
        // FNA Compat is just that...temp compat. Anyone using it should really do dynamic
        // per frame vertex update properly :). 
        if (_dynamic && _timesCalledPerFrame >= 1)
        {
            _activeVertexBufferIndex++;

            if (_vertexBuffers.Count == _activeVertexBufferIndex)
            {
                AddVertexBuffer();
            }
            RhiVertexBuffer = _vertexBuffers[_activeVertexBufferIndex];
        }

        RhiVertexBuffer.SetData(offsetInBytes, data, dataLength);
        RhiVertexBuffer.Upload();

        if(_dynamic)
        {
            _timesCalledPerFrame++;
        }
    }

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    internal void Reset()
    {
        _activeVertexBufferIndex = 0;
        _timesCalledPerFrame = 0;
    }

    protected VertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage, bool dynamic)
    {
        _graphicsDevice = graphicsDevice;
        _vertexDeclaration = vertexDeclaration;
        _vertexCount = vertexCount;
        _dynamic = dynamic;

        if(_dynamic)
        {
            graphicsDevice.RegisterDynamicVertexBuffer(this);
        }

        AddVertexBuffer();

        RhiVertexBuffer = _vertexBuffers[0];
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        foreach(var vertexBuffer in _vertexBuffers)
        {
            vertexBuffer.CleanUp();
        }

        if (_dynamic)
        {
            _graphicsDevice.UnregisterDynamicVertexBuffer(this);
        }

        _disposed = true;
    }

    private void AddVertexBuffer()
    {
        RhiVertexBufferFlags flags = RhiVertexBufferFlags.None;

        if(_dynamic)
        {
            flags |= RhiVertexBufferFlags.Dynamic;
        }

        var vertexBuffer = _graphicsDevice.RenderResourceFactory.CreateVertexBuffer(new RhiVertexBufferDescription
        {
            VertexCount = (uint)_vertexCount,
            Stride = _vertexDeclaration.RhiVertexDefinition.Stride,
            AttributesDefinition = _vertexDeclaration.RhiVertexDefinition,
            Flags = flags
        });

        _vertexBuffers.Add(vertexBuffer);
    }
}
