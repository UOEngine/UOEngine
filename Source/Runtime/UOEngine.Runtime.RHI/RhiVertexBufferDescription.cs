// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.RHI;

[Flags]
public enum RhiVertexBufferFlags
{
    None = 0,
    Dynamic
}

public struct RhiVertexBufferDescription
{
    public uint VertexCount;
    public RhiVertexDefinition AttributesDefinition;
    public uint Stride;
    public RhiVertexBufferFlags Flags;
}
