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
