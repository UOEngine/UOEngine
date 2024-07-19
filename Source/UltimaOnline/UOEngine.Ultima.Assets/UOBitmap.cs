using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.UltimaOnline.Assets
{
    public class UOBitmap
    {
        public UOBitmap() 
        {
            Texels = [];
        }

        public UOBitmap(uint width, uint  height, ushort[] texels)
        {
            Width = width; 
            Height = height;
            Texels = texels;
        }

        public ushort[] Texels { get; private set; }

        public uint Width { get; private set; }
        public uint Height { get; private set; }
    }
}
