using System.Diagnostics;

using Silk.NET.Vulkan;

namespace UOEngine.Runtime.Rendering
{
    public static class RenderCommon
    {
        public static Format TextureFormatToVulkanFormat(ERenderTextureFormat textureFormat)
        {
            Format format = Format.Undefined;

            switch(textureFormat)
            {
                case ERenderTextureFormat.R5G5B5A1:
                    {
                        format = Format.R5G5B5A1UnormPack16;

                        break;
                    }

                case ERenderTextureFormat.A1R5G5B5:
                    {
                        format = Format.A1R5G5B5UnormPack16;

                        break;
                    }

                case ERenderTextureFormat.R8G8B8A8:
                    {
                        format = Format.R8G8B8A8Unorm;

                        break;
                    }

                case ERenderTextureFormat.B8G8R8A8:
                    {
                        format = Format.B8G8R8A8Unorm;

                        break;
                    }

                default:
                    {
                        break;
                    }
            }

            Debug.Assert(format != Format.Undefined);

             return format;
        }

        public static ERenderTextureFormat VulkanFormatToTextureFormat(Format vulkanFormat)
        {
            ERenderTextureFormat textureFormat = ERenderTextureFormat.None;

            switch(vulkanFormat)
            {
                default:
                    {
                        break;
                    }
            }

            Debug.Assert(textureFormat != ERenderTextureFormat.None);

            return textureFormat;
        }

    }
}
