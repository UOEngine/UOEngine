// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.RHI;

public enum ShaderProgramType: byte
{
    Vertex,
    Pixel,
    Compute,
    Count,
    Invalid
}

public static class ShaderProgramTypeExtensions
{
    public static int ToInt(this ShaderProgramType shaderProgramType)
    {
        return (int)shaderProgramType;
    }
}