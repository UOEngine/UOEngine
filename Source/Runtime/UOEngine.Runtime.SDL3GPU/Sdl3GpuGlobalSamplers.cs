using System.Diagnostics;

using UOEngine.Runtime.RHI;
using UOEngine.Runtime.SDL3GPU.Resources;

namespace UOEngine.Runtime.SDL3GPU;

internal class Sdl3GpuGlobalSamplers
{
    public readonly Sdl3GpuSampler PointClamp;

    private readonly Sdl3GpuDevice _device;
    private readonly Dictionary<int, Sdl3GpuSampler> _globalSamplers = [];

    public Sdl3GpuGlobalSamplers(Sdl3GpuDevice device)
    {
        _device = device;

        PointClamp = RegisterGlobalSampler(new RhiSampler { Filter = RhiSamplerFilter.Point });
    }

    public Sdl3GpuSampler GetSampler(RhiSampler rhiSampler)
    {
        Debug.Assert(rhiSampler.Filter != RhiSamplerFilter.Invalid);

        return _globalSamplers[rhiSampler.GetHashCode()];
    }

    private Sdl3GpuSampler RegisterGlobalSampler(RhiSampler rhiSampler)
    {
        var globalSampler = new Sdl3GpuSampler(_device, rhiSampler);

        _globalSamplers.Add(globalSampler.Description.GetHashCode(), globalSampler);

        return globalSampler;
    }
}
