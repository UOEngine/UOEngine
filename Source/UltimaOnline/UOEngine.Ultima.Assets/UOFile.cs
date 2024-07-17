using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using UOEngine.Runtime.Core;

namespace UOEngine.UltimaOnline.Assets
{
    public class UOFile
    {
        public UOFile(string filePath)
        {
            _path = filePath;
        }

        public unsafe void Load()
        {
            FileInfo fileInfo = new FileInfo(_path);

            Deserialiser? deserialiser;

            long size = fileInfo.Length;

            if (size > 0)
            {
                _file = MemoryMappedFile.CreateFromFile
                (
                    File.Open(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
                    null,
                    0,
                    MemoryMappedFileAccess.Read,
                    HandleInheritability.None,
                    false
                );

                _accessor = _file.CreateViewAccessor(0, size, MemoryMappedFileAccess.Read);

                byte* ptr = null;

                try
                {
                    _accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);

                    deserialiser = new Deserialiser(ptr, _accessor.SafeMemoryMappedViewHandle.ByteLength);
                }
                catch
                {
                    Debug.Assert(false);

                    _accessor.SafeMemoryMappedViewHandle.ReleasePointer();

                    return;
                }
            }
            else
            {
                Debug.Assert(false);

                return;
            }

            if (deserialiser.ReadUInt() != UOFileMagicNumber)
            {
                throw new ArgumentException("Bad UO file");
            }

            uint version = deserialiser.ReadUInt();
            uint format_timestamp = deserialiser.ReadUInt();
            long nextBlock = deserialiser.ReadLong();
            uint block_size = deserialiser.ReadUInt();
            int count = deserialiser.ReadInt();

            Debug.Assert(false);
        }

        private string                       _path;
        private MemoryMappedViewAccessor?    _accessor;
        private MemoryMappedFile?            _file;

        private const uint UOFileMagicNumber = 0x50594D;

    }
}
