// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.RHI;

public class RhiRenderTarget
{
    public uint Width => Texture.Width;
    public uint Height => Texture.Height;

    public IRenderTexture Texture { get; private set; } = null!;

    public void Setup(IRenderTexture texture)
    {
        Texture = texture;
    }
}
