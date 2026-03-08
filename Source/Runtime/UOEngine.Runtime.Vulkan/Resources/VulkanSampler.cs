// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Vortice.Vulkan;

using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Vulkan;

internal class VulkanSampler: IDisposable
{
    public VkSampler Handle { get; private set; }

    private readonly VulkanDevice _device;
    private bool _disposed;

    internal VulkanSampler(VulkanDevice device, RhiSampler rhiSampler)
    {
        _device = device;

        VkSamplerCreateInfo samplerCreateInfo = new()
        {
            magFilter = VkFilter.Linear,
            minFilter = VkFilter.Linear,
            addressModeU = VkSamplerAddressMode.ClampToEdge,
            addressModeV = VkSamplerAddressMode.ClampToEdge,
            addressModeW = VkSamplerAddressMode.ClampToEdge,
            anisotropyEnable = true,
            maxAnisotropy = device.DeviceInfo.DeviceProperties.limits.maxSamplerAnisotropy,
            unnormalizedCoordinates = false,
            compareEnable = false,
            compareOp = VkCompareOp.Always
        };

        device.Api.vkCreateSampler(device.Handle, samplerCreateInfo, out var sampler);

        Handle = sampler;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
            }

            _device.Api.vkDestroySampler(_device.Handle, Handle);

            Handle = VkSampler.Null;

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~VulkanSampler()
    {
        Dispose(disposing: false);
    }
}
