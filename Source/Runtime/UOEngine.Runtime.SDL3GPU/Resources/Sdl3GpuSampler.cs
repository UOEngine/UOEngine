// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using static SDL3.SDL;

using UOEngine.Runtime.RHI;
using UOEngine.Runtime.Core;

namespace UOEngine.Runtime.SDL3GPU.Resources;

internal static class SamplerExtensionMethods
{
    public static SDL_GPUFilter ToSdl3GpuFilter(this RhiSamplerFilter filter)
    {
        switch(filter)
        {
            case RhiSamplerFilter.Point: return SDL_GPUFilter.SDL_GPU_FILTER_NEAREST;
            case RhiSamplerFilter.Bilinear: return SDL_GPUFilter.SDL_GPU_FILTER_LINEAR;
            default:
                throw new NotImplementedException("Unimplemented Sampler filter.");
        }
    }
}

internal class Sdl3GpuSampler: Sdl3GpuResource
{
    public readonly RhiSampler Description;

    private readonly SDL_GPUSamplerCreateInfo _samplerCreateInfo;

    public Sdl3GpuSampler(Sdl3GpuDevice device, in RhiSampler description, string? debugName = null)
        : base(device, null, debugName)
    {
        Description = description;

        _samplerCreateInfo = new SDL_GPUSamplerCreateInfo
        {
            min_filter = description.Filter.ToSdl3GpuFilter(),
            mag_filter = description.Filter.ToSdl3GpuFilter(),
            address_mode_u = MapAddressMode(description.AddressMode),
            address_mode_v = MapAddressMode(description.AddressMode),
            address_mode_w = MapAddressMode(description.AddressMode),
        };

        Handle = SDL_CreateGPUSampler(device.Handle, _samplerCreateInfo);

        UOEDebug.Assert(Handle != IntPtr.Zero);

    }

    protected override void FreeResource()
    {
        SDL_ReleaseGPUSampler(Device.Handle, Handle);
    }

    private static SDL_GPUSamplerAddressMode MapAddressMode(RhiTextureAddressMode m) => m switch
    {
        RhiTextureAddressMode.Clamp => SDL_GPUSamplerAddressMode.SDL_GPU_SAMPLERADDRESSMODE_CLAMP_TO_EDGE,
        _ => throw new NotImplementedException(),
    };
}
