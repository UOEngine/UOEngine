using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

namespace UOEngine.UOAssets
{
    public ref struct UOBitmap
    {
        public Span<uint> Texels;
        public uint Width = 0;
        public uint Height = 0;

        public bool IsEmpty => this == Empty();

        public UOBitmap(uint width, uint height, Span<uint> texels)
        {
            Width = width;
            Height = height;
            Texels = texels;
        }

        public static bool operator == (UOBitmap left, UOBitmap right)
        {
            if(left.Width != right.Height)
            {
                return false;
            }

            if (left.Height != right.Height)
            {
                return false;
            }

            if(left.Texels == right.Texels)
            {
                return true;
            }

            return true;
        }

        public static bool operator != (UOBitmap left, UOBitmap right)
        {
            return !(left == right);
        }

        public void DeserialiseFromUOPackageFileResource(byte[] data)
        {
            using var reader = new BinaryReader(new MemoryStream(data));

            Width = reader.ReadUInt32();
            Height = reader.ReadUInt32();

            //if (info.Width <= 0)
            //{
            //    info.Width = (int)width;
            //}

            //if (info.Height <= 0)
            //{
            //    info.Height = (int)height;
            //}

            Texels = new uint[Width * Height];
            var len = reader.BaseStream.Length - reader.BaseStream.Position;
            var halfLen = len >> 2;
            var start = reader.BaseStream.Position;
            var rowLookup = new int[Height];

            reader.Read(MemoryMarshal.AsBytes<int>(rowLookup.AsSpan()));

            ushort color = 0;// info.Hue;

            for (var y = 0; y < Height; ++y)
            {
                reader.BaseStream.Position = (start + (rowLookup[y] << 2));

                var pixelIndex = (int)(y * Width);
                var gsize = (y < Height - 1) ? rowLookup[y + 1] - rowLookup[y] : halfLen - rowLookup[y];

                for (var i = 0; i < gsize; ++i)
                {
                    var value = reader.ReadUInt16();
                    var run = reader.ReadUInt16();
                    var rbga = 0u;

                    if (color != 0 && value != 0)
                    {
                        throw new NotImplementedException();
                        //value = FileManager.Hues.GetColor16(value, color);
                    }

                    if (value != 0)
                    {
                        rbga = HuesHelper.Color16To32(value) | 0xFF_00_00_00;
                    }

                    Texels.Slice(pixelIndex, run).Fill(rbga);
                    pixelIndex += run;
                }
            }
        }

        [SupportedOSPlatform("windows")]
        public unsafe void DebugDumpAsBitmap(string filename)
        {
            //var texelBytes = new byte[Texels.Length * 2];

            //uint i = 0;

            //foreach (var _texel in Texels)
            //{
            //    texelBytes[i++] = (byte)(_texel & 0xFF);
            //    texelBytes[i++] = (byte)(_texel >> 8);
            //}

            //using (Bitmap bmp = new Bitmap(filename))
            //using (Graphics g = Graphics.FromImage(bmp))
            //{
            //    g.Clear(Color.White);
            //    g.DrawString("Hello PNG!", new Font("Arial", 16), Brushes.Black, new PointF(10, 40));

            //    bmp.Save("output.png", System.Drawing.Imaging.ImageFormat.Png);
            //}

            //using (Bitmap bmp = new Bitmap((int)Width, (int)Height, PixelFormat.Format32bppArgb))
            //{
            //    for (int y = 0; y < Height; y++)
            //    {
            //        for (int x = 0; x < Width; x++)
            //        {
            //            uint value = Texels[y * (int)Width + x];

            //            Color.From

            //            bmp.SetPixel(x, y, (int)Texels[y * (int)Width + x]);

            //            bmp.Set
            //        }
            //    }

            //    bmp.Save(filename, ImageFormat.Png);
            //}

            using var bitmap = new Bitmap((int)Width, (int)Height, PixelFormat.Format32bppArgb);

            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, (int)Width, (int)Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);


            int stride = bitmapData.Stride;
            int bytesPerPixel = 4;

            // If texels are in ARGB (0xAARRGGBB), reorder to BGRA
            byte[] buffer = new byte[stride * Height];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    uint argb = Texels[y * (int)Width + x];
                    byte a = (byte)(argb >> 24);
                    byte r = (byte)(argb >> 16);
                    byte g = (byte)(argb >> 8);
                    byte b = (byte)(argb);

                    int idx = y * stride + x * bytesPerPixel;
                    buffer[idx + 0] = b;
                    buffer[idx + 1] = g;
                    buffer[idx + 2] = r;
                    buffer[idx + 3] = a;
                }
            }

            Marshal.Copy(buffer, 0, bitmapData.Scan0, buffer.Length);

            bitmap.UnlockBits(bitmapData);

            bitmap.Save(filename, ImageFormat.Png);
        }

        public static UOBitmap Empty()
        {
            return new UOBitmap(0, 0, []);
        }
    }
}
