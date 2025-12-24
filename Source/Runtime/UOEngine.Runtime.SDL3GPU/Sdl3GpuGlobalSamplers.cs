// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
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

        PointClamp = RegisterGlobalSampler(RhiSampler.PointClamp);
        LinearClamp = RegisterGlobalSampler(RhiSampler.Bilinear);

        //_device.RegisterOnDeviceCreatedCallback(() =>
        //{
        //    PointClamp = RegisterGlobalSampler(RhiSampler.PointClamp);
        //    LinearClamp = RegisterGlobalSampler(RhiSampler.Bilinear);
        //});
    }

    public Sdl3GpuSampler GetSampler(RhiSampler rhiSampler)
    {
        Debug.Assert(rhiSampler.Filter != RhiSamplerFilter.Invalid);

        return _globalSamplers[rhiSampler];
    }

    public Sdl3GpuSampler RegisterGlobalSampler(RhiSampler rhiSampler)
    {
        var globalSampler = new Sdl3GpuSampler(_device, rhiSampler);

        _globalSamplers.Add(rhiSampler, globalSampler);

        return globalSampler;
    }
}
