using System.Diagnostics;
using System.Runtime.InteropServices;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using UOEngine.Interop;

namespace UOEngine;

public class Texture2D
{
    public UIntPtr          NativeHandle { get; private set; } = UIntPtr.Zero;
    public readonly uint    Width ;
    public readonly uint    Height;

    public string           Name { get; set; }

    private Memory<Colour>  _pixels;

    public Texture2D(uint width, uint height, string name = "")
    {
        Debug.Assert(width > 0);
        Debug.Assert(height > 0);

        _pixels = new Colour[width * height];

        Width = width;
        Height = height;
        Name = name;
    }

    public unsafe void Apply()
    {
        if(NativeHandle == UIntPtr.Zero)
        {
            NativeHandle = RendererInterop.CreateTexture(Width, Height, Name);
        }

        ReadOnlySpan<byte> pixelBytes = MemoryMarshal.AsBytes(_pixels.Span);

        fixed(byte* start = pixelBytes)
        {
            RendererInterop.SetTextureData(NativeHandle, (UIntPtr)start, (uint)pixelBytes.Length);
        }
    }

    public void SetPixel(uint x, uint y, Colour colour) 
    {
        int index = (int)(y * Width + x);

        _pixels.Span[index] = colour;
    }

    public void SetPixel(uint x, uint y, uint colour)
    {
        int index = (int)(y * Width + x);

        _pixels.Span[index] = new Colour(colour);
    }

    public void SetPixels(ReadOnlySpan<uint> texels)
    {
        for(int i = 0; i < texels.Length; i++)
        {
            _pixels.Span[i] = new Colour(texels[i]);
        }
    }

    public void SetPixels(Colour colour)
    {
        foreach(ref Colour texel in _pixels.Span)
        {
            texel = colour;
        }
    }

    public Colour GetPixel(uint x, uint y)
    {
        uint index = y * Width + x;

        return GetPixel(index);
    }

    public Colour GetPixel(uint index)
    {
        return _pixels.Span[(int)index];
    }

    public void SaveAsPng(string filename)
    {
        using var image = new Image<Rgba32>((int)Width, (int)Height);

        for (uint y = 0; y < Height; y++)
        {
            for (uint x = 0; x < Width; x++)
            {
                var pixel = GetPixel(x, y);

                image[(int)x, (int)y] = new Rgba32(pixel.R, pixel.G, pixel.B, pixel.A);
            }
        }

        image.SaveAsPng(filename);
    }
}
