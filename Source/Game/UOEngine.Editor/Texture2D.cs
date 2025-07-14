using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UOEngine.Interop;

namespace UOEngine
{
    public class Texture2D
    {
        readonly UIntPtr        _nativeHandle;

        private Memory<Colour>  _pixels;

        public Texture2D(uint width, uint height)
        {
            _pixels = new Colour[width * height];

            Width = width;
            Height = height;

            _nativeHandle = RendererInterop.CreateTexture((int)width, (int)height);
        }

        public unsafe void Apply()
        {
            ReadOnlySpan<byte> pixelBytes = MemoryMarshal.AsBytes(_pixels.Span);

            fixed(byte* start = pixelBytes)
            {
                RendererInterop.SetTextureData(_nativeHandle, (UIntPtr)start, pixelBytes.Length);
            }
        }

        public void SetPixel(uint x, uint y, Colour colour) 
        {
            int index = (int)(y * Width + x);

            _pixels.Span[index] = colour;
        }

        public uint Width { get; private set; }
        public uint Height { get; private set; }


    }
}
