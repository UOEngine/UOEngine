namespace UOEngine.Runtime.Core
{
    public class Deserialiser
    {
        public unsafe Deserialiser(byte* data, ulong length) 
        {
            _data = data;
            Length = length;
            Position = 0;
        }

        public unsafe long ReadLong()
        {
            long value = *(long*)(_data + Position);

            Position += 8;

            return value;
        }

        public unsafe int ReadInt()
        {
            int value = *(int*)(_data + Position);

            Position += 4;

            return value;
        }

        public unsafe uint ReadUInt()
        {
            uint value = *(uint*)(_data + Position);

            Position += 4;

            return value;
        }

        public ulong            Position { get; set; }

        public ulong            Length { get; private set; }

        private unsafe byte*    _data;
    }
}
