// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Text;

using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Renderer;

//public class ScreenQuadVertexBuffer : VertexBuffer<PositionUVVertex>
//{
//    public ScreenQuadVertexBuffer(IRenderResourceFactory factory, in VertexBufferDescription description)
//        :base(factory, description)
//    {

//    }

//    public ScreenQuadVertexBuffer(IRenderResourceFactory factory, in VertexBufferDescription description)
//    : base(factory, description)
//    {

//    }
//}
//}

public class ScreenTriangleIndexBuffer: IndexBuffer
{
    public ScreenTriangleIndexBuffer(IRenderResourceFactory factory)
        : base(factory, 3)
    {
        SetData([0, 1, 3]);
    }
}