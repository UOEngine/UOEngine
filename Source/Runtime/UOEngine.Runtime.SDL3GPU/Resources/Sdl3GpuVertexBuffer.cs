// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Runtime.InteropServices;

using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.SDL3GPU;

internal class Sdl3GpuVertexBuffer: Sdl3GpuBuffer, IRhiVertexBuffer
{
    public readonly RhiVertexDefinition? VertexDefinition;

    public uint Size => (uint)Data.Length;

    public uint Stride { get; private set; }

    public Sdl3GpuVertexBuffer(Sdl3GpuDevice device, in RhiVertexBufferDescription description)
        :base(device, RhiBufferType.Vertex, description.VertexCount * description.Stride)
    {
        VertexDefinition = description.AttributesDefinition;
        Stride = description.Stride;
    }

    internal Sdl3GpuVertexBuffer(Sdl3GpuDevice device, uint lengthBytes)
        :base(device, RhiBufferType.Vertex, lengthBytes)
    {
        VertexDefinition = null;
    }

    public void SetData(ReadOnlySpan<ushort> data) => data.CopyTo(MemoryMarshal.Cast<byte, ushort>(Data.AsSpan()));

    public void CleanUp()
    {
        Dispose();
    }
}
