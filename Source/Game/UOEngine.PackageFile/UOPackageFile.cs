using System.Diagnostics;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;

namespace UOEngine.PackageFile;

public static class UOFileExtensionMethods
{
    public static unsafe T Read<T>(this BinaryReader reader) where T : unmanaged
    {
        Unsafe.SkipInit<T>(out var value);

        var p = new Span<byte>(&value, sizeof(T));

        reader.Read(p);

        return value;
    }
}

public enum CompressionType : ushort
{
    None,
    Zlib,
    ZlibBwt = 3
}

public class UOPackageFile : IDisposable
{
    public bool IsUop { get; private set; }

    public ReadOnlySpan<UOFileIndex> FileIndices => _fileIndices;

    public MemoryMappedViewStream? Stream { get; private set; }

    public BinaryReader? Reader { get; private set; }

    public bool Exists => _fileInfo.Exists;

    public bool IsMounted => Reader != null;

    public int NumResources { get; private set; } = 0;

    private const uint UOFileMagicNumber = 0x50594D;

    private UOFileIndex[] _fileIndices = [];
    private MemoryMappedFile? _file;
    private FileInfo _fileInfo;
    private UOPackageFile? _idxFile;

    public UOPackageFile(string path)
    {
        _fileInfo = new FileInfo(path);

        IsUop = _fileInfo.Extension == ".uop";
    }

    public UOPackageFile(string path, string idxFilePath)
    {
        _fileInfo = new FileInfo(path);
        _idxFile = new UOPackageFile(idxFilePath);

        Debug.Assert(_fileInfo.Extension == ".mul");
        Debug.Assert(idxFilePath.EndsWith(".idx"));
        Debug.Assert(_fileInfo.Exists);
    }

    public void Dispose()
    {
        Stream?.Dispose();
        _file?.Dispose();
    }

    public void Mount()
    {
        long size = _fileInfo.Length;

        if (size > 0)
        {
            _file = MemoryMappedFile.CreateFromFile
            (
                File.Open(_fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
                null,
                0,
                MemoryMappedFileAccess.Read,
                HandleInheritability.None,
                false
            );
        }
        else
        {
            Debug.Assert(false);

            return;
        }

        Stream = _file.CreateViewStream(0, size, MemoryMappedFileAccess.Read);

        Reader = new BinaryReader(Stream);
    }

    public void Load(string packedFilePattern, bool bHasExtra)
    {
        if(IsMounted == false)
        {
            Mount();
        }
        //long size = _fileInfo.Length;

        //if (size > 0)
        //{
        //    _file = MemoryMappedFile.CreateFromFile
        //    (
        //        File.Open(_fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
        //        null,
        //        0,
        //        MemoryMappedFileAccess.Read,
        //        HandleInheritability.None,
        //        false
        //    );
        //}
        //else
        //{
        //    Debug.Assert(false);

        //    return;
        //}

        //Stream = _file.CreateViewStream(0, size, MemoryMappedFileAccess.Read);
        {
            string uopPattern = Path.GetFileNameWithoutExtension(_fileInfo.Name).ToLowerInvariant();

            Reader = new BinaryReader(Stream);

            if (_fileInfo.Extension == ".mul")
            {
                LoadMulFile(Reader);

                return;
            }

            if (_fileInfo.Extension == ".uop")
            {
                LoadUopFile(Reader, Stream, packedFilePattern, bHasExtra);

                return;
            }
        }
    }

    public byte[] GetResource(int idx)
    {
        var info = _fileIndices[idx];

        if (info.CompressionFlag != CompressionType.ZlibBwt && info.Width <= 0 && info.Height <= 0)
        {
            throw new NotImplementedException("");
        }

        using MemoryMappedViewStream stream = _file!.CreateViewStream(0, _fileInfo!.Length, MemoryMappedFileAccess.Read);

        stream.Seek(info.Offset, SeekOrigin.Begin);

        ReadOnlySpan<byte> mem = null;

        var decompressedBuffer = new byte[info.DecompressedLength];

        if (info.CompressionFlag >= CompressionType.Zlib)
        {
            using var decompressor = new ZLibStream(stream, CompressionMode.Decompress);
            using var output = new MemoryStream(decompressedBuffer);

            decompressor.CopyTo(output);

            if (info.CompressionFlag == CompressionType.ZlibBwt)
            {
                decompressedBuffer = BwtDecompress.Decompress(decompressedBuffer);
            }

            mem = decompressedBuffer;
        }

        return decompressedBuffer;
    }

    public static ulong CreateHash(string s)
    {
        uint eax, ecx, edx, ebx, esi, edi;

        eax = ecx = edx = ebx = esi = edi = 0;
        ebx = edi = esi = (uint)s.Length + 0xDEADBEEF;

        int i = 0;

        for (i = 0; i + 12 < s.Length; i += 12)
        {
            edi = (uint)((s[i + 7] << 24) | (s[i + 6] << 16) | (s[i + 5] << 8) | s[i + 4]) + edi;
            esi = (uint)((s[i + 11] << 24) | (s[i + 10] << 16) | (s[i + 9] << 8) | s[i + 8]) + esi;
            edx = (uint)((s[i + 3] << 24) | (s[i + 2] << 16) | (s[i + 1] << 8) | s[i]) - esi;

            edx = (edx + ebx) ^ (esi >> 28) ^ (esi << 4);
            esi += edi;
            edi = (edi - edx) ^ (edx >> 26) ^ (edx << 6);
            edx += esi;
            esi = (esi - edi) ^ (edi >> 24) ^ (edi << 8);
            edi += edx;
            ebx = (edx - esi) ^ (esi >> 16) ^ (esi << 16);
            esi += edi;
            edi = (edi - ebx) ^ (ebx >> 13) ^ (ebx << 19);
            ebx += esi;
            esi = (esi - edi) ^ (edi >> 28) ^ (edi << 4);
            edi += ebx;
        }

        if (s.Length - i > 0)
        {
            switch (s.Length - i)
            {
                case 12:
                    esi += (uint)s[i + 11] << 24;
                    goto case 11;
                case 11:
                    esi += (uint)s[i + 10] << 16;
                    goto case 10;
                case 10:
                    esi += (uint)s[i + 9] << 8;
                    goto case 9;
                case 9:
                    esi += s[i + 8];
                    goto case 8;
                case 8:
                    edi += (uint)s[i + 7] << 24;
                    goto case 7;
                case 7:
                    edi += (uint)s[i + 6] << 16;
                    goto case 6;
                case 6:
                    edi += (uint)s[i + 5] << 8;
                    goto case 5;
                case 5:
                    edi += s[i + 4];
                    goto case 4;
                case 4:
                    ebx += (uint)s[i + 3] << 24;
                    goto case 3;
                case 3:
                    ebx += (uint)s[i + 2] << 16;
                    goto case 2;
                case 2:
                    ebx += (uint)s[i + 1] << 8;
                    goto case 1;
                case 1:
                    ebx += s[i];
                    break;
            }

            esi = (esi ^ edi) - ((edi >> 18) ^ (edi << 14));
            ecx = (esi ^ ebx) - ((esi >> 21) ^ (esi << 11));
            edi = (edi ^ ecx) - ((ecx >> 7) ^ (ecx << 25));
            esi = (esi ^ edi) - ((edi >> 16) ^ (edi << 16));
            edx = (esi ^ ecx) - ((esi >> 28) ^ (esi << 4));
            edi = (edi ^ edx) - ((edx >> 18) ^ (edx << 14));
            eax = (esi ^ edi) - ((edi >> 8) ^ (edi << 24));

            return ((ulong)edi << 32) | eax;
        }

        return ((ulong)esi << 32) | eax;
    }

    private void LoadMulFile(BinaryReader reader)
    {
        Debug.Assert(_fileInfo.Extension == ".mul");

        UOPackageFile file = _idxFile ?? this;

        int count = (int)_fileInfo.Length / 12;

        _fileIndices = new UOFileIndex[count];

        for (int i = 0; i < _fileIndices.Length; i++)
        {
            ref UOFileIndex fileIndex = ref _fileIndices[i];

            //fileIndex.File = this;  
            fileIndex.Offset = reader.ReadUInt32();
            fileIndex.Length = reader.ReadInt32();
            fileIndex.DecompressedLength = 0;

            int size = reader.ReadInt32();

            if (size > 0)
            {
                fileIndex.Width = (short)(size >> 16);
                fileIndex.Height = (short)(size & 0xFFFF);
            }
        }
    }

    private void LoadUopFile(BinaryReader reader, MemoryMappedViewStream stream, string packedFilePattern, bool bHasExtra)
    {
        Debug.Assert(_fileInfo.Extension == ".uop");

        stream.Seek(0, SeekOrigin.Begin);

        _fileIndices = new UOFileIndex[ushort.MaxValue];

        if (reader.ReadUInt32() != UOFileMagicNumber)
        {
            throw new ArgumentException("Bad UO file");
        }

        uint version = reader.ReadUInt32();
        uint signature = reader.ReadUInt32();
        long nextBlock = reader.ReadInt64();
        uint blockSize = reader.ReadUInt32();
        int count = reader.ReadInt32();

        NumResources = 0;

        Dictionary<ulong, int> hashes = new Dictionary<ulong, int>();

        for (int i = 0; i < count; i++)
        {
            string  entryName = string.Format(packedFilePattern, i);
            ulong   hash = CreateHash(entryName);

            if (!hashes.ContainsKey(hash))
            {
                hashes.Add(hash, i);
            }
        }

        stream.Seek(nextBlock, SeekOrigin.Begin);

        int realTotal = 0;

        while (nextBlock != 0)
        {
            int filesCount = reader.ReadInt32();
            nextBlock = reader.ReadInt64();

            NumResources += filesCount;

            for (int i = 0; i < filesCount; i++)
            {
                long offset = reader.ReadInt64();
                int headerLength = reader.ReadInt32();
                int compressedLength = reader.ReadInt32();
                int decompressedLength = reader.ReadInt32();
                ulong hash = reader.ReadUInt64();
                uint checksumAdler32 = reader.ReadUInt32();
                short flag = reader.ReadInt16();

                int dataSize = flag == 1 ? compressedLength : decompressedLength;

                if (offset == 0)
                {
                    continue;
                }

                if (hashes.TryGetValue(hash, out int idx) == false)
                {
                    continue;
                }

                realTotal++;
                //offset += headerLength;

                var fileIndex = new UOFileIndex();

                fileIndex.Length = dataSize;
                fileIndex.Offset = offset + headerLength;

                if (bHasExtra && flag != 3)
                {
                    long curpos = stream.Position;

                    stream.Seek(offset + headerLength, SeekOrigin.Begin);

                    // width and height for gumps.
                    fileIndex.Width = (short)reader.ReadInt32();
                    fileIndex.Height = (short)reader.ReadInt32();

                    stream.Seek(curpos, SeekOrigin.Begin);

                    fileIndex.Offset += 8;
                }
                else
                {
                    fileIndex.Length = compressedLength;
                    fileIndex.DecompressedLength = decompressedLength;
                    fileIndex.CompressionFlag = (CompressionType)flag;
                }

                _fileIndices[idx] = fileIndex;
            }

            stream.Seek(nextBlock, SeekOrigin.Begin);
        }
    }
}
