// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.RHI;

[Flags]
public enum RhiBufferUsageFlags
{
    None = 0,

    Static,
    Dynamic,

    Vertex,
    Index
}

public struct RhiBufferDescription
{
    public uint Size;
    public uint Stride;
    public RhiBufferUsageFlags Usage;
    public byte[] InitialData;
    public string Name;
}

public interface IRhiBuffer
{
    public void SetData<T>(ReadOnlySpan<T> data) where T : unmanaged;
}
