// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;

namespace UOEngine.Runtime.RHI;

[DebuggerDisplay("RhiRenderTarget")]
public class RhiRenderTarget
{
    public uint Width => Texture.Width;
    public uint Height => Texture.Height;

    public readonly string? Name;

    public IRenderTexture Texture { get; private set; } = null!;

    public RhiRenderTarget(string? name = null)
    {
        Name = name;
    }

    public void Setup(IRenderTexture texture)
    {
        Texture = texture;
    }
}
