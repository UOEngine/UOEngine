using System.Diagnostics;

using UOEngine.Runtime.RHI;
using UOEngine.Runtime.SDL3GPU.Resources;

namespace UOEngine.Runtime.SDL3GPU;

internal class Sdl3GpuGlobalSamplers
{
    public readonly Sdl3GpuSampler PointClamp;
    public readonly Sdl3GpuSampler LinearClamp;

    private readonly Sdl3GpuDevice _device;
    private readonly Dictionary<RhiSampler, Sdl3GpuSampler> _globalSamplers = [];

    public Sdl3GpuGlobalSamplers(Sdl3GpuDevice device)
    {
        _device = device;

        PointClamp = RegisterGlobalSampler(new RhiSampler { Filter = RhiSamplerFilter.Point });
        LinearClamp = RegisterGlobalSampler(new RhiSampler { Filter = RhiSamplerFilter.Bilinear });
    }

    public Sdl3GpuSampler GetSampler(RhiSampler rhiSampler)
    {
        Debug.Assert(rhiSampler.Filter != RhiSamplerFilter.Invalid);

        return _globalSamplers[rhiSampler];
    }

    private Sdl3GpuSampler RegisterGlobalSampler(RhiSampler rhiSampler)
    {
        var globalSampler = new Sdl3GpuSampler(_device, rhiSampler);

        _globalSamplers.Add(rhiSampler, globalSampler);

        return globalSampler;
    }
}
