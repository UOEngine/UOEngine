// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

[DebuggerDisplay("{_name}")]
internal class VulkanCommandBufferPool
{
    internal readonly VkCommandPool Handle;

    internal uint SubmissionFrame = uint.MaxValue;

    private readonly uint _queueIndex;
    private readonly VulkanDevice _device;

    private readonly List<VulkanCommandBuffer> _commandBuffers = [];

    private readonly List<VulkanCommandBuffer> _freeCommandBuffers = [];

    private readonly List<VulkanCommandBuffer> _inUseCommandBuffers = [];

    private static int _numPools = 0;

    private readonly string _name;

    internal unsafe VulkanCommandBufferPool(VulkanDevice device, uint queueFamilyIndex)
    {
        _device = device;

        VkCommandPoolCreateInfo commandPoolCreateInfo = new()
        {
            queueFamilyIndex = queueFamilyIndex,
            flags = VkCommandPoolCreateFlags.ResetCommandBuffer
        };

        _device.Api.vkCreateCommandPool(_device.Handle, &commandPoolCreateInfo, out var commandPool);

        Handle = commandPool;

        _name = $"VulkanCommandBufferPool{_numPools++}";


    }
    internal void Reset()
    {
        _device.Api.vkResetCommandPool(_device.Handle, Handle, VkCommandPoolResetFlags.None);

        foreach(var commandBuffer in _inUseCommandBuffers)
        {
            _freeCommandBuffers.Add(commandBuffer);
        }

        _inUseCommandBuffers.Clear();
    }

    internal VulkanCommandBuffer Create()
    {
        VulkanCommandBuffer? commandBuffer = null;

        if (_freeCommandBuffers.Count > 0)
        {
            commandBuffer = _freeCommandBuffers.Last();

            _freeCommandBuffers.RemoveAt(_freeCommandBuffers.Count - 1);
        }

        if(commandBuffer == null)
        {
            commandBuffer = new VulkanCommandBuffer(_device, this);

            _commandBuffers.Add(commandBuffer);
        }

        _inUseCommandBuffers.Add(commandBuffer);

        return commandBuffer;
    }
}
