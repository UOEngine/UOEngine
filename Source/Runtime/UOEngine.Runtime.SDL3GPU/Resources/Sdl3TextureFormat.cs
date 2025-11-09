using System.Diagnostics;

using static SDL3.SDL;

using UOEngine.Runtime.RHI.Resources;

namespace UOEngine.Runtime.SDL3GPU.Resources;

internal static class Sdl3TextureFormat
{
    public static SDL_GPUTextureFormat ToSdl3GpuFormat(this TextureFormat textureFormat)
    {
        switch (textureFormat)
        {
            case TextureFormat.R8G8B8A8_UNorm: return SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_R8G8B8A8_UNORM;
            default:
                {
                    Debug.Assert(false);
                    break;
                }
        }

        return SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_INVALID;
    }

    public static TextureFormat ToRhiFormat(this SDL_GPUTextureFormat textureFormat)
    {
        switch (textureFormat)
        {
            case SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_R8G8B8A8_UNORM: return TextureFormat.R8G8B8A8_UNorm;
            default:
                {
                    Debug.Assert(false);
                    break;
                }
        }

        return TextureFormat.Invalid;
    }
}
