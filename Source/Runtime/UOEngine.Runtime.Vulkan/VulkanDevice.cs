// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace UOEngine.Runtime.Vulkan;

public struct VulkanQueueInfo
{
    public uint Index;
    public VkQueueFlags Flags;
}

public struct VulkanDeviceInfo
{
    public VkPhysicalDevice PhysicalDevice;
    public VkPhysicalDeviceProperties DeviceProperties;
    public VulkanQueueInfo[] Queues;
}

enum VulkanQueueType
{
    Graphics,
    Copy,
    Compute,
    Count,
    Invalid
}

//[Service(UOEServiceLifetime.Singleton)]
public class VulkanDevice : IDisposable
{
    public readonly VulkanDeviceInfo _deviceInfo;

    public VkDeviceApi Api => _api ?? throw new InvalidOperationException("");

    private VkDevice? _device;
    private VkDeviceApi? _api;

    private VulkanQueue[] _queues = new VulkanQueue[(int)VulkanQueueType.Count];

    public VulkanDevice(in VulkanDeviceInfo deviceInfo)
    {
        _deviceInfo = deviceInfo;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public unsafe void InitGpu(VkInstanceApi instanceApi)
    {

        List<VkUtf8String> enabledExtensions = [VK_KHR_SWAPCHAIN_EXTENSION_NAME];

        using var deviceExtensionNames = new VkStringArray(enabledExtensions);

        VkDeviceQueueCreateInfo* queueCreateInfos = stackalloc VkDeviceQueueCreateInfo[_deviceInfo.Queues.Length];

        uint queueCount = 0;
        float priority = 1.0f;

        foreach (var queueInfo in _deviceInfo.Queues)
        {
            queueCreateInfos[queueCount++] = new VkDeviceQueueCreateInfo
            {
                queueFamilyIndex = queueInfo.Index,
                queueCount = 1,
                pQueuePriorities = &priority
            };
        }

        VkDeviceCreateInfo deviceCreateInfo = new()
        {
            pNext = null,
            queueCreateInfoCount = (uint)_deviceInfo.Queues.Length,
            pQueueCreateInfos = queueCreateInfos,
            enabledExtensionCount = deviceExtensionNames.Length,
            ppEnabledExtensionNames = deviceExtensionNames,
            pEnabledFeatures = null,
        };

        VkDevice device;

        instanceApi.vkCreateDevice(_deviceInfo.PhysicalDevice, &deviceCreateInfo, &device).CheckResult();

        _device = device;

        _api = GetApi(instanceApi.Instance, device);

        for(uint i = 0; i < (int)VulkanQueueType.Count; i++)
        {
            _api.vkGetDeviceQueue(device, i, 0, out var queue);

            _queues[i] = new VulkanQueue((VulkanQueueType)i, queue);
        }
    }


}
