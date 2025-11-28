using static SDL3.SDL;

using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.SDL3GPU.Resources;

internal static class SamplerExtensionMethods
{
    public static SDL_GPUFilter ToSdl3GpuFilter(this SamplerFilter filter)
    {
        switch(filter)
        {
            case SamplerFilter.Point: return SDL_GPUFilter.SDL_GPU_FILTER_NEAREST;
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
            mag_filter = description.Filter.ToSdl3GpuFilter()
        };

        Handle = SDL_CreateGPUSampler(device.Handle, _samplerCreateInfo);

    }

    protected override void FreeResource()
    {
        SDL_ReleaseGPUSampler(Device.Handle, Handle);
    }
}
