using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UOEngine.Interop;

namespace UOEngine
{
    public class Texture2D
    {
        public Texture2D(uint width, uint height)
        {
            pixels = new Colour[width * height];

            Width = width;
            Height = height;

            nativeHandle = RendererInterop.CreateTexture((int)width, (int)height);
        }

        public unsafe void Apply()
        {
            fixed(Colour* colour = pixels.Span)
            {
                //NativeMethods.SetTextureInitialData(nativeHandle, colour, pixels.Span.Length);

            }
        }

        public void SetPixel(uint x, uint y, Colour colour) 
        {
            int index = (int)(y * Width + x);

            pixels.Span[index] = colour;
        }

        public uint Width { get; private set; }
        public uint Height { get; private set; }

        readonly UIntPtr nativeHandle;

        Memory<Colour> pixels;
    }
}
