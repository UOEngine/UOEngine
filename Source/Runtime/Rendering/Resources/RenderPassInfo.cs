using UOEngine.Runtime.Core;

namespace UOEngine.Runtime.Rendering.Resources
{
    public readonly struct RenderPassInfo
    {
        public readonly ERenderTextureFormat TextureFormat;
        public RenderPassInfo(ERenderTextureFormat textureFormat)
        {
            TextureFormat = textureFormat;
        }

        public readonly uint GetHash()
        {
            Crc32 crc = new();

            crc.Append((uint)TextureFormat);

            return crc.GetValue();
        }
    }
}
