// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Text;

using UOEngine.Runtime.Core;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Renderer;

public enum DefaultTextureType
{
    Red,
    Green,
    Blue,
    RedCheckerboard,

    Count
}

public class GlobalRenderResources
{
    public IRenderTexture GetDefaultTexture(DefaultTextureType type) => _defaultTextures[(int)type];

    private IRenderTexture[] _defaultTextures = new IRenderTexture[(int)DefaultTextureType.Count];

    private readonly IRenderResourceFactory _resourceFactory;

    internal GlobalRenderResources(IRenderResourceFactory resourceFactory)
    {
        _resourceFactory = resourceFactory;
    }

    internal void Init()
    {
        uint defaultTextureSize = 128;

        CreateDefaultTexture(DefaultTextureType.Red, defaultTextureSize, defaultTextureSize, Colour.Red, FillWithSolidColour, "DefaultRedTexture");
        CreateDefaultTexture(DefaultTextureType.Green, defaultTextureSize, defaultTextureSize, Colour.Green, FillWithSolidColour, "DefaultGreenTexture");
        CreateDefaultTexture(DefaultTextureType.Blue, defaultTextureSize, defaultTextureSize, Colour.Blue, FillWithSolidColour, "DefaultBlueTexture");
        CreateDefaultTexture(DefaultTextureType.RedCheckerboard, defaultTextureSize, defaultTextureSize, Colour.Red, FillWithCheckerboardEffect, "DefaultRedCheckerboardTexture");
    }

    private void CreateDefaultTexture(DefaultTextureType type, uint width, uint height, in Colour colour, Action<uint, uint, Colour, Span<Colour>> pixelFillFunction, string? name = null)
    {
        var texture = _resourceFactory.CreateTexture(new RhiTextureDescription
        {
            Width = width,
            Height = height,
            Name = name,
            Usage = RhiRenderTextureUsage.Sampler,
        });

        Span<Colour> texels = texture.GetTexelsAs<Colour>();

        pixelFillFunction(width, height, colour, texels);

        texture.Upload();

        _defaultTextures[(int)type] = texture;
    }

    private void FillWithSolidColour(uint width, uint height, Colour colour, Span<Colour> texels) => texels.Fill(colour);

    private void FillWithCheckerboardEffect(uint width, uint height, Colour colour, Span<Colour> texels)
    {
        int checkSize = 16;

        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < height; x++)
            {
                bool isWhite = ((x / checkSize) + (y / checkSize)) % 2 == 0;

                int i = y * (int)width + x;

                if (isWhite)
                {
                    texels[i] = Colour.White;
                }
                else
                {
                    texels[i] = colour;
                }
            }
        }
    }
}
