using System.Diagnostics;
using System.Runtime.InteropServices;

using UOEngine.Interop;

namespace UOEngine;

public class Texture2D
{
    public readonly UIntPtr NativeHandle;
    public readonly uint    Width ;
    public readonly uint    Height;

    private Memory<Colour>  _pixels;

    public Texture2D(uint width, uint height)
    {
        Debug.Assert(width > 0);
        Debug.Assert(height > 0);

        _pixels = new Colour[width * height];

        Width = width;
        Height = height;

        NativeHandle = RendererInterop.CreateTexture((int)width, (int)height);
    }

    public unsafe void Apply()
    {
        ReadOnlySpan<byte> pixelBytes = MemoryMarshal.AsBytes(_pixels.Span);

        fixed(byte* start = pixelBytes)
        {
            RendererInterop.SetTextureData(NativeHandle, (UIntPtr)start, pixelBytes.Length);
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
}
