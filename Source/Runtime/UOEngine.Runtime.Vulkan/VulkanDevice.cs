// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Vulkan;

internal struct VulkanQueueInfo
{
    public uint Index;
    public VkQueueFlags Flags;
}

internal struct VulkanDeviceInfo
{
    public VkPhysicalDevice PhysicalDevice;
    public VkPhysicalDeviceProperties DeviceProperties;
    public VkPhysicalDeviceMemoryProperties MemoryProperties;
    public VulkanQueueInfo[] Queues;
}

internal enum VulkanQueueType
{
    Graphics,
    Copy,
    Compute,
    Count,
    Invalid
}

internal class VulkanDevice : IDisposable
{
    internal readonly VulkanDeviceInfo DeviceInfo;
    internal VkPhysicalDevice PhysicalDeviceHandle => DeviceInfo.PhysicalDevice;
    internal VkDevice Handle => _device ?? throw new InvalidOperationException("VulkanDevice: VkDevice is not initialised.");

    internal VulkanQueue PresentQueue => GetQueue(VulkanQueueType.Graphics);
    internal VulkanQueue CopyQueue => GetQueue(VulkanQueueType.Copy);
    internal VulkanQueue GraphicsQueue => GetQueue(VulkanQueueType.Graphics);
    internal VkDeviceApi Api => _api ?? throw new InvalidOperationException("VulkanDevice: Api is not initialised.");

    internal readonly VulkanStagingBuffer StagingBuffer;

    internal readonly VulkanMemoryManager MemoryManager;
    internal readonly VulkanDeviceMemoryManager DeviceMemoryManager;

    internal readonly VulkanDeferredDeletionQueue DeferredDeletionQueue;

    internal VulkanGraphicsContext GraphicsContext = null!;
    internal VkPhysicalDeviceProperties DeviceProperties { get; private set; }
    internal VkPhysicalDeviceLimits Limits => DeviceProperties.limits;

    private VkDevice? _device;
    private VkDeviceApi? _api;

    private VulkanQueue[] _queues = new VulkanQueue[(int)VulkanQueueType.Count];

    private List<VulkanSemaphore> _semaphores = [];

    internal VulkanDevice(in VulkanDeviceInfo deviceInfo)
    {
        DeviceInfo = deviceInfo;
        MemoryManager = new(this);
        DeviceMemoryManager = new(this);
        StagingBuffer = new(this);
        DeferredDeletionQueue = new(this);
    }

    internal VulkanQueue GetQueue(VulkanQueueType type) => _queues[(int)type];

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public unsafe void InitGpu(VkInstanceApi instanceApi)
    {
        List<VkUtf8String> enabledExtensions = 
        [
            VK_KHR_SWAPCHAIN_EXTENSION_NAME,
            VK_KHR_MAINTENANCE_1_EXTENSION_NAME,
        ];

        using var deviceExtensionNames = new VkStringArray(enabledExtensions);

        VkDeviceQueueCreateInfo* queueCreateInfos = stackalloc VkDeviceQueueCreateInfo[DeviceInfo.Queues.Length];

        uint queueCount = 0;
        float priority = 1.0f;

        foreach (var queueInfo in DeviceInfo.Queues)
        {
            queueCreateInfos[queueCount++] = new VkDeviceQueueCreateInfo
            {
                queueFamilyIndex = queueInfo.Index,
                queueCount = 1,
                pQueuePriorities = &priority
            };
        }

        VkPhysicalDeviceDynamicRenderingFeatures dynamicRenderingFeatures = new()
        {
            dynamicRendering = true
        };

        VkPhysicalDeviceSynchronization2Features sync2 = new()
        {
            synchronization2 = true,
            pNext = &dynamicRenderingFeatures
        };

        VkPhysicalDeviceFeatures enabledFeatures = new()
        {
            samplerAnisotropy = true
        };

        VkDeviceCreateInfo deviceCreateInfo = new()
        {
            pNext = &sync2,
            queueCreateInfoCount = (uint)DeviceInfo.Queues.Length,
            pQueueCreateInfos = queueCreateInfos,
            enabledExtensionCount = deviceExtensionNames.Length,
            ppEnabledExtensionNames = deviceExtensionNames,
            pEnabledFeatures = &enabledFeatures,
        };

        VkDevice device;

        instanceApi.vkCreateDevice(DeviceInfo.PhysicalDevice, &deviceCreateInfo, &device).CheckResult();

        _device = device;

        _api = GetApi(instanceApi.Instance, device);

        for(uint i = 0; i < (int)VulkanQueueType.Count; i++)
        {
            _api.vkGetDeviceQueue(device, i, 0, out var queue);

            _queues[i] = new VulkanQueue(this, (VulkanQueueType)i, queue);
        }

        VkPhysicalDeviceProperties2 deviceProperties2 = new();

        instanceApi.vkGetPhysicalDeviceProperties2(DeviceInfo.PhysicalDevice, &deviceProperties2);

        DeviceProperties = deviceProperties2.properties;

        MemoryManager.Init();

        StagingBuffer.Init();
    }

    public void WaitForGpuIdle() => Api.vkDeviceWaitIdle(Handle);

    public uint GetMemoryTypeIndex(uint typeBits, VkMemoryPropertyFlags properties)
    {
        for (int i = 0; i < DeviceInfo.MemoryProperties.memoryTypeCount; i++)
        {
            if ((typeBits & 1) == 1)
            {
                if ((DeviceInfo.MemoryProperties.memoryTypes[i].propertyFlags & properties) == properties)
                {
                    return (uint)i;
                }
            }
            typeBits >>= 1;
        }

        throw new Exception("Could not find a suitable memory type!");
    }

    public VkSemaphore CreateSemaphore()
    {
        Api.vkCreateSemaphore(Handle, out var semaphore).CheckResult();

        return semaphore;
    }

    internal VulkanSemaphore CreateSemaphore(in RhiSemaphoreDescription description)
    {
        var semaphore = new VulkanSemaphore(description, this);

        _semaphores.Add(semaphore);

        return semaphore;
    }
}
