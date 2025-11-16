using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using UOEngine.Runtime.RHI;

using static SDL3.SDL;

namespace UOEngine.Runtime.SDL3GPU.Resources;

internal struct SDL3GPUTextureDescription
{
    public SDL_GPUTextureCreateInfo CreateInfo;
    public string Name;
}

internal class SDL3GPUTexture: Sdl3GpuResource, IRenderTexture
{
    public uint Width => Description.width;
    public uint Height => Description.height;

    public readonly SDL_GPUTextureCreateInfo Description;

    public readonly byte[] Texels;

    public SDL3GPUTexture(Sdl3GpuDevice device, in SDL3GPUTextureDescription description)
        : base(device, SDL_SetGPUTextureName, description.Name)
    {
        Description = description.CreateInfo;

        uint bytesPerTexel = 0;

        switch(Description.format)
        {
            case SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_R8G8B8A8_UNORM:
            case SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_B8G8R8A8_UNORM:
                {
                    bytesPerTexel = 4;
                    break;
                }
            default:
                throw new Exception("");
        }

        Texels = new byte[Description.width * Description.height * bytesPerTexel];
    }

    public void Init()
    {
        var createInfo = new SDL_GPUTextureCreateInfo()
        {
            type = SDL_GPUTextureType.SDL_GPU_TEXTURETYPE_2D,
            usage = SDL_GPUTextureUsageFlags.SDL_GPU_TEXTUREUSAGE_SAMPLER,
            format = SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_R8G8B8A8_UNORM,
            width = Description.width,
            height = Description.height,
            layer_count_or_depth = 1,
            num_levels = 1,
            sample_count = SDL_GPUSampleCount.SDL_GPU_SAMPLECOUNT_1,
            props = 0,
        };

        Handle = SDL_CreateGPUTexture(Device.Handle, createInfo);
    }

    public void InitFromExistingResource(IntPtr _handle)
    {
        Handle = _handle;
    }

    public Span<T> GetTexelsAs<T>() where T: unmanaged
    {
        return MemoryMarshal.Cast<byte, T>(Texels);
    }

    public void Upload()
    {
        uint size = (uint)Texels.Length;

        IntPtr transferBuffer = SDL_CreateGPUTransferBuffer(Device.Handle, new SDL_GPUTransferBufferCreateInfo
        {
            size = size,
            usage = SDL_GPUTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD
        });

        IntPtr mappedMemory = SDL_MapGPUTransferBuffer(Device.Handle, transferBuffer, false);

        unsafe
        {
            fixed (void* src = Texels)
            {
                Buffer.MemoryCopy(src, (void*)mappedMemory, size, size);
            }
        }

        SDL_UnmapGPUTransferBuffer(Device.Handle, mappedMemory);

        IntPtr commandBuffer = SDL_AcquireGPUCommandBuffer(Device.Handle);
        IntPtr copyPass = SDL_BeginGPUCopyPass(commandBuffer);

        var transferInfo = new SDL_GPUTextureTransferInfo
        {
            transfer_buffer = transferBuffer,
            offset = 0,
        };

        var textureRegion = new SDL_GPUTextureRegion
        {
            texture = Handle,
            w = Description.height,
            h = Description.width,
            d = 1
        };

        SDL_UploadToGPUTexture(copyPass, transferInfo, textureRegion, false);

        SDL_EndGPUCopyPass(copyPass);
        SDL_SubmitGPUCommandBuffer(commandBuffer);
        SDL_UnmapGPUTransferBuffer(Device.Handle, transferBuffer);
    }

    protected override void FreeResource()
    {
        SDL_DestroyTexture(Handle);
    }
}
