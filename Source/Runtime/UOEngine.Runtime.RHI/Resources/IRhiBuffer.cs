// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.RHI;

[Flags]
public enum RhiBufferUsageFlags
{
    None = 0,

    Static = 1,
    Dynamic = 2,

    Vertex = 4,
    Index = 8
}

public struct RhiBufferDescription
{
    // Number of bytes in the buffer
    public uint Size;

    // The stride in bytes of the buffer.
    public uint Stride;
    public RhiBufferUsageFlags Usage;
    public byte[] InitialData;
    public string Name;
}

public interface IRhiBuffer
{
    public RhiBufferDescription Description { get; }

    public void SetData<T>(ReadOnlySpan<T> data) where T : unmanaged;

    public Span<byte> Lock(uint size, uint offset);

    public void Unlock();
}
