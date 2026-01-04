// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;

namespace UOEngine.Runtime.RHI;

[DebuggerDisplay("{Offset}, {Type}")]
//public class RhiVertexAttribute
public readonly record struct RhiVertexAttribute
{
    public readonly RhiVertexAttributeType Type;
    public readonly RhiVertexAttributeFormat Format;
    public readonly uint Offset;

    public RhiVertexAttribute(RhiVertexAttributeType type, RhiVertexAttributeFormat format, uint offset)
    {
        Type = type;
        Format = format;
        Offset = offset;
    }
}
