// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace UOEngine.Runtime.Image;

public class Image2
{
    public static void SaveAsPng(string filename, ReadOnlySpan<byte> texels, uint width, uint height)
    {
        using var img = SixLabors.ImageSharp.Image.LoadPixelData<Rgba32>(texels, (int)width, (int)height);

        img.SaveAsPng(filename);

    }
}
