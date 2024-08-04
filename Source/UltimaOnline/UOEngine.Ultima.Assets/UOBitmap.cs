using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

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

        [SupportedOSPlatform("windows")]
        public unsafe void DebugDumpAsBitmap(string filename)
        {

            var texelBytes = new byte[_texels.Length * 2];

            uint i = 0;

            foreach (var _texel in _texels)
            {
                texelBytes[i++] = (byte)(_texel & 0xFF);
                texelBytes[i++] = (byte)(_texel >> 8);
            }

            var bitmap = new Bitmap((int)Width, (int)Height, PixelFormat.Format16bppArgb1555);

            BitmapData bd = bitmap.LockBits(
                new Rectangle(0, 0, (int)Width, (int)Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

            Marshal.Copy(texelBytes, 0, bd.Scan0, texelBytes.Length);

            bitmap.UnlockBits(bd);

            bitmap.Save(filename);
        }

        public ReadOnlySpan<ushort> Texels => new ReadOnlySpan<ushort>(_texels);

        public readonly uint Width;
        public readonly uint Height;

        private ushort[] _texels;
    }
}
