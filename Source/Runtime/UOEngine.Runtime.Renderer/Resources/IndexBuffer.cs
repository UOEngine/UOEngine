// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Renderer;

public class IndexBuffer
{
    public readonly IRhiBuffer RhiBuffer;

    private ushort[] _indices = [];
   
    internal IndexBuffer(IRenderResourceFactory factory, uint size)
    {
        RhiBuffer = factory.NewBuffer(new RhiBufferDescription
        {
            Size = size,
            Stride = sizeof(ushort),
            Usage = RhiBufferUsageFlags.Index | RhiBufferUsageFlags.Dynamic
        });
    }

    public void SetData(ushort[] indices)
    {
        _indices = indices;

        RhiBuffer.SetData<ushort>(indices);
    }
}
