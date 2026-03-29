// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Text;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Renderer;

internal class BlitTextureShaderInstance
{
    internal readonly ShaderInstance ShaderInstance;

    internal IRenderTexture Texture
    {
        set
        {
            _texture = value;
            ShaderInstance.SetTexture(_textureBindingHandle, _texture);
        }
    }

    private readonly ShaderBindingHandle _textureBindingHandle;
    private IRenderTexture? _texture;

    internal BlitTextureShaderInstance(ShaderInstance blitTextureShaderInstance)
    {
        ShaderInstance = blitTextureShaderInstance;

        _textureBindingHandle = ShaderInstance.GetBindingHandleTexturePixel("Texture");
    }
}
