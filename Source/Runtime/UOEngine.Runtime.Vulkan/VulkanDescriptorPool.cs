// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System;
using UOEngine.Runtime.Core;
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

internal class VulkanDescriptorPool
{
    private readonly VulkanDevice _device;

    private readonly VkDescriptorPool _descriptorPool;

    private readonly static VkDescriptorType[] _typesForAllocation = 
    [
        VkDescriptorType.Sampler,
        VkDescriptorType.SampledImage,
        VkDescriptorType.UniformBuffer
    ];

    internal unsafe VulkanDescriptorPool(VulkanDevice device)
    {
        _device = device;

        const uint numDescriptorsAvailable = 64;
        const uint maxSets = 64;

        VkDescriptorPoolSize* descriptorPoolSizes = stackalloc VkDescriptorPoolSize[_typesForAllocation.Length];

        for(int i = 0; i < _typesForAllocation.Length; i++)
        {
            descriptorPoolSizes[i].type = _typesForAllocation[i];
            descriptorPoolSizes[i].descriptorCount = numDescriptorsAvailable;
        }

        VkDescriptorPoolCreateInfo descriptorPoolCreateInfo = new()
        {
            poolSizeCount = (uint)_typesForAllocation.Length,
            maxSets = maxSets,
            pPoolSizes = descriptorPoolSizes
        };

        _device.Api.vkCreateDescriptorPool(_device.Handle, descriptorPoolCreateInfo, out _descriptorPool);
    }

    internal unsafe VkDescriptorSet Allocate(VkDescriptorSetLayout descriptorSetLayout)
    {
        VkDescriptorSetLayout layout = descriptorSetLayout;

        VkDescriptorSetAllocateInfo descriptorSetAllocatorInfo = new()
        {
            descriptorPool = _descriptorPool,
            descriptorSetCount = 1,
            pSetLayouts = &layout
        };

        VkDescriptorSet descriptorSet;

        VkResult result = _device.Api.vkAllocateDescriptorSets(_device.Handle, &descriptorSetAllocatorInfo, &descriptorSet);

        if(result != VkResult.Success)
        {
            UOEDebug.Assert(false);
        }

        return descriptorSet;
    }

    internal void Reset()
    {
        _device.Api.vkResetDescriptorPool(_device.Handle, _descriptorPool, VkDescriptorPoolResetFlags.None);
    }
}
