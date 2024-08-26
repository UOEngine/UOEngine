
namespace UOEngine.Runtime.Core
{
    public class Crc32
    {
        private readonly System.IO.Hashing.Crc32 _crc32;

        public Crc32()
        {
            _crc32 = new System.IO.Hashing.Crc32();
        }

        public void Append(uint value)
        {
            _crc32.Append(BitConverter.GetBytes(value));
        }

        public uint GetValue()
        {
            return _crc32.GetCurrentHashAsUInt32();
        }
    }
}
