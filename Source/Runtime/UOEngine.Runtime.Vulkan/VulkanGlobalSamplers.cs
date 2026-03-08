// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;

using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Vulkan;

internal class VulkanGlobalSamplers
{
    public readonly VulkanSampler PointClamp;
    public readonly VulkanSampler LinearClamp;

    private readonly VulkanDevice _device;

    private readonly Dictionary<RhiSampler, VulkanSampler> _globalSamplers = [];

    public VulkanGlobalSamplers(VulkanDevice device)
    {
        _device = device;

        PointClamp = RegisterGlobalSampler(RhiSampler.PointClamp);
        LinearClamp = RegisterGlobalSampler(RhiSampler.Bilinear);
    }

    public VulkanSampler GetSampler(RhiSampler rhiSampler)
    {
        Debug.Assert(rhiSampler.Filter != RhiSamplerFilter.Invalid);

        return _globalSamplers[rhiSampler];
    }

    public VulkanSampler RegisterGlobalSampler(RhiSampler rhiSampler)
    {
        var globalSampler = new VulkanSampler(_device, rhiSampler);

        _globalSamplers.Add(rhiSampler, globalSampler);

        return globalSampler;
    }
}
