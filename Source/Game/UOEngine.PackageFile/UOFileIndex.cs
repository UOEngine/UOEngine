namespace UOEngine.PackageFile
{
    public struct UOFileIndex
    {
        public UOFileIndex
        (
            IntPtr address,
            uint fileSize,
            long offset,
            int length,
            int decompressed,
            CompressionType compressionFlag = CompressionType.None,
            short width = 0,
            short height = 0,
            ushort hue = 0
        )
        {
            Address = address;
            FileSize = fileSize;
            Offset = offset;
            Length = length;
            DecompressedLength = decompressed;
            Width = width;
            Height = height;
            Hue = hue;

            AnimOffset = 0;
        }

        public IntPtr           Address;
        public uint             FileSize;
        public long             Offset;
        public int              Length;
        public int              DecompressedLength;
        public CompressionType  CompressionFlag;
        public int              Width;
        public int              Height;
        public ushort           Hue;
        public sbyte            AnimOffset;

        //public ulong LookUp;

        public static UOFileIndex Invalid = new UOFileIndex
        (
            IntPtr.Zero,
            0,
            0,
            0,
            0
        );

        public bool Equals(UOFileIndex other)
        {
            return (Address, Offset, Length, DecompressedLength) == (other.Address, other.Offset, other.Length, other.DecompressedLength);
        }
    }
}
