// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.RHI;

public enum RhiVertexAttributeFormat
{
    Float,
    Vector2,
    Vector3,
    Vector4,
    R8G8B8A8_UNorm,
    UInt32,
}

public static class RhiVertexAttributeFormatExtensions
{
    public static uint ToSize(this RhiVertexAttributeFormat f) => (f) switch
    {
        RhiVertexAttributeFormat.Float => 4,
        RhiVertexAttributeFormat.Vector2 => 8,
        RhiVertexAttributeFormat.Vector3 => 12,
        RhiVertexAttributeFormat.Vector4 => 16,
        RhiVertexAttributeFormat.R8G8B8A8_UNorm => 4,
        _ => throw new NotSupportedException()
    };
}

