// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.SDL3GPU;

internal class Sdl3GpuDynamicVertexStream
{
    private readonly Sdl3GpuVertexBuffer _vertexBuffer;
    private int _writeOffsetBytes;

    public Sdl3GpuDynamicVertexStream(Sdl3GpuDevice device)
    {
        _vertexBuffer = new Sdl3GpuVertexBuffer(device, new RHI.RhiVertexBufferDescription
        {
            VertexCount = 65556
        });
    }

    public void BeginFrame()
    {
        _writeOffsetBytes = 0;
    }

    public void Bind()
    {

    }
}
