// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Runtime.CompilerServices;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Renderer;

[Service(UOEServiceLifetime.Singleton)]
public class RendererResourcesFactory
{
    private readonly IRenderResourceFactory _rhiResourceFactory;

    public RendererResourcesFactory(IRenderResourceFactory factory)
    {
        _rhiResourceFactory = factory;
    }

    public IndexBuffer NewIndexBuffer(uint size, string? name = default)
    {
        return new IndexBuffer(_rhiResourceFactory, size);
    }

    public VertexBuffer<T> NewVertexBuffer<T>(uint size) where T: unmanaged, IVertex, IVertexLayoutProvider
    {
        return new VertexBuffer<T>(_rhiResourceFactory, size);
    }
}
