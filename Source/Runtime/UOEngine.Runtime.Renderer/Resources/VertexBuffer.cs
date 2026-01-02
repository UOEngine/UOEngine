// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Renderer;

public class VertexBuffer
{
    public readonly IRhiBuffer RhiBuffer;



    internal VertexBuffer(IRenderResourceFactory factory, uint size, uint stride)
    {
        RhiBuffer = factory.NewBuffer(new RhiBufferDescription
        {
            Size = size * stride,
            Stride = stride,
            Usage = RhiBufferUsageFlags.Vertex | RhiBufferUsageFlags.Dynamic
        });
    }

    public void SetData<T>(T[] data) where T: unmanaged
    {
        RhiBuffer.SetData<T>(data.AsSpan());
    }

}
