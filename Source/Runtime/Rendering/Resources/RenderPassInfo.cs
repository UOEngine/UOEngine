using System.IO.Hashing;

namespace UOEngine.Runtime.Rendering.Resources
{
    public readonly struct RenderPassInfo
    {
        public readonly ERenderTextureFormat TextureFormat;
        public RenderPassInfo(ERenderTextureFormat textureFormat)
        {
            TextureFormat = textureFormat;
        }

        //public readonly uint GetHash()
        //{
        //    Crc32 crc = new Crc32();

        //    crc.Append(TextureFormat);

        //    return crc.GetCurrentHashAsUInt32();
        //}
    }
}
