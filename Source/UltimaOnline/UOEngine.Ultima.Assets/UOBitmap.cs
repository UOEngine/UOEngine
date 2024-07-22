using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.UltimaOnline.Assets
{
    public class UOBitmap
    {
        public UOBitmap(uint width, uint  height, ushort[] texels)
        {
            Width = width; 
            Height = height;
            _texels = texels;
        }

        public ReadOnlySpan<ushort> Texels => new ReadOnlySpan<ushort>(_texels);

        public readonly uint Width;
        public readonly uint Height;

        private ushort[] _texels;
    }
}
