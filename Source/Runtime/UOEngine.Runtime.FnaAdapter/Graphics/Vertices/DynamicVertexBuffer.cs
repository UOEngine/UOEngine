namespace Microsoft.Xna.Framework.Graphics;

public class DynamicVertexBuffer: VertexBuffer
{
    public DynamicVertexBuffer(GraphicsDevice graphicsDevice, Type type, int vertexCount, BufferUsage bufferUsage)
        : base(graphicsDevice, VertexDeclaration.FromType(type), vertexCount, bufferUsage)
    {
        throw new NotImplementedException();
    }
}
