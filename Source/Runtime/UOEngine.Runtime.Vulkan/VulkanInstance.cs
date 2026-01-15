// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace UOEngine.Runtime.Vulkan;

internal unsafe class VulkanInstance
{
    public VkInstance Instance => _instance;

    public VkInstanceApi Api => _instanceApi ?? throw new InvalidOperationException("");

    private VkInstance _instance;
    private VkInstanceApi? _instanceApi;
    private VkDebugUtilsMessengerEXT _debugMessenger = VkDebugUtilsMessengerEXT.Null;

    public void Create(string applicationName, bool enableDebug)
    {
        HashSet<VkUtf8String> availableInstanceLayers = [.. EnumerateInstanceLayers()];
        HashSet<VkUtf8String> availableInstanceExtensions = [.. GetInstanceExtensions()];

        List<VkUtf8String> instanceExtensions = [];

        uint count;
        byte** strings = UOEngineSdl3.SDL_Vulkan_GetInstanceExtensions(&count);
        string[] names = new string[count];
        for (int i = 0; i < count; i++)
        {
            ReadOnlySpan<byte> sdlExtSpan = Encoding.UTF8.GetBytes(Marshal.PtrToStringUTF8((nint)strings[i])!);
            instanceExtensions.Add(sdlExtSpan);
        }

        List<VkUtf8String> instanceLayers = [];

        if (enableDebug)
        {
            instanceLayers.Add(VK_LAYER_KHRONOS_VALIDATION_EXTENSION_NAME);
        }

        foreach (VkUtf8String availableExtension in availableInstanceExtensions)
        {
            if (availableExtension == VK_EXT_DEBUG_UTILS_EXTENSION_NAME)
            {
                instanceExtensions.Add(VK_EXT_DEBUG_UTILS_EXTENSION_NAME);
            }
            else if (availableExtension == VK_EXT_SWAPCHAIN_COLOR_SPACE_EXTENSION_NAME)
            {
                instanceExtensions.Add(VK_EXT_SWAPCHAIN_COLOR_SPACE_EXTENSION_NAME);
            }
        }

        VkUtf8ReadOnlyString pApplicationName = Encoding.UTF8.GetBytes(applicationName);
        VkUtf8ReadOnlyString pEngineName = "UOEngine"u8;

        VkApplicationInfo appInfo = new()
        {
            sType = VkStructureType.ApplicationInfo,
            pApplicationName = pApplicationName,
            applicationVersion = new VkVersion(1, 0, 0),
            pEngineName = pEngineName,
            engineVersion = new VkVersion(1, 0, 0),
            apiVersion = VkVersion.Version_1_3
        };

        using VkStringArray vkLayerNames = new(instanceLayers);
        using VkStringArray vkInstanceExtensions = new(instanceExtensions);

        VkInstanceCreateInfo createInfo = new()
        {
            sType = VkStructureType.InstanceCreateInfo,
            pApplicationInfo = &appInfo,
            enabledLayerCount = vkLayerNames.Length,
            ppEnabledLayerNames = vkLayerNames,
            enabledExtensionCount = vkInstanceExtensions.Length,
            ppEnabledExtensionNames = vkInstanceExtensions
        };

        VkDebugUtilsMessengerCreateInfoEXT debugUtilsCreateInfo = new();

        if (instanceLayers.Count > 0)
        {
            debugUtilsCreateInfo.messageSeverity = VkDebugUtilsMessageSeverityFlagsEXT.Error | VkDebugUtilsMessageSeverityFlagsEXT.Warning;
            debugUtilsCreateInfo.messageType = VkDebugUtilsMessageTypeFlagsEXT.Validation | VkDebugUtilsMessageTypeFlagsEXT.Performance;
            debugUtilsCreateInfo.pfnUserCallback = &DebugMessengerCallback;
            createInfo.pNext = &debugUtilsCreateInfo;
        }

        VkInstance instance;

        VkResult result = vkCreateInstance(&createInfo, null, &instance);
        result.CheckResult();

        _instanceApi = GetApi(instance);
        _instance = instance;

        if (instanceLayers.Count > 0)
        {
            _instanceApi.vkCreateDebugUtilsMessengerEXT(instance, &debugUtilsCreateInfo, null, out _debugMessenger).CheckResult();
        }

        Console.WriteLine($"Created VkInstance with version: {appInfo.apiVersion.Major}.{appInfo.apiVersion.Minor}.{appInfo.apiVersion.Patch}");

        if (instanceLayers.Count > 0)
        {
            foreach (var layer in instanceLayers)
            {
                Console.WriteLine($"Instance layer '{layer}'");
            }
        }

        foreach (VkUtf8String extension in instanceExtensions)
        {
            Console.WriteLine($"Instance extension '{extension}'");
        }
    }

    public void Destroy() => Api.vkDestroyInstance(_instance);

    public VulkanDeviceInfo GetSuitableDevice()
    {
        uint physicalDevicesCount = 0;
        
        Api.vkEnumeratePhysicalDevices(_instance, &physicalDevicesCount, null).CheckResult();

        if (physicalDevicesCount == 0)
        {
            throw new Exception("Vulkan: Failed to find GPUs with Vulkan support");
        }

        Span<VkPhysicalDevice> physicalDevices = stackalloc VkPhysicalDevice[(int)physicalDevicesCount];

        Api.vkEnumeratePhysicalDevices(_instance, physicalDevices).CheckResult();

        VkPhysicalDevice physicalDevice = default;
        VkPhysicalDeviceProperties deviceProperties = default;

        for (int i = 0; i < physicalDevicesCount; i++)
        {
            physicalDevice = physicalDevices[i];

            Api.vkGetPhysicalDeviceProperties(physicalDevice, out deviceProperties);

            bool discrete = deviceProperties.deviceType == VkPhysicalDeviceType.DiscreteGpu;

            if(discrete)
            {
                // Prefer discrete of course.
                break;
            }
        }

        // Query for Vulkan 1.3 features
        VkPhysicalDeviceVulkan13Features queryVulkan13Features = new();
        VkPhysicalDeviceFeatures2 queryDeviceFeatures2 = new();
        queryDeviceFeatures2.pNext = &queryVulkan13Features;
        Api.vkGetPhysicalDeviceFeatures2(physicalDevice, &queryDeviceFeatures2);

        // Check if Physical device supports Vulkan 1.3 features
        if (queryVulkan13Features.dynamicRendering == false)
        {
            throw new NotSupportedException("Dynamic Rendering feature is missing");
        }

        if (queryVulkan13Features.synchronization2 == false)
        {
            throw new NotSupportedException("Synchronization2 feature is missing");
        }

        // Fill in info with chosen device.
        Api.vkEnumerateDeviceExtensionProperties(physicalDevice, out uint propertyCount).CheckResult();

        Span<VkExtensionProperties> availableDeviceExtensions = stackalloc VkExtensionProperties[(int)propertyCount];

        Api.vkEnumerateDeviceExtensionProperties(physicalDevice, availableDeviceExtensions).CheckResult();


        var deviceInfo = new VulkanDeviceInfo
        {
            PhysicalDevice = physicalDevice,
            DeviceProperties = deviceProperties
        };

        Api.vkGetPhysicalDeviceMemoryProperties(physicalDevice, out deviceInfo.MemoryProperties);

        Api.vkGetPhysicalDeviceQueueFamilyProperties(physicalDevice, out uint queueFamilyCount);
        Span<VkQueueFamilyProperties> queueFamilies = stackalloc VkQueueFamilyProperties[(int)queueFamilyCount];
        Api.vkGetPhysicalDeviceQueueFamilyProperties(physicalDevice, queueFamilies);

        if (FindQueue(queueFamilies, VkQueueFlags.Graphics, VkQueueFlags.None, out var graphicsQueue) == false)
        {
            throw new InvalidOperationException("No graphics queue for device?");
        }

        // Have we a dedicated copy queue for async copies?
        if(FindQueue(queueFamilies, VkQueueFlags.Transfer, VkQueueFlags.Graphics | VkQueueFlags.Compute, out var copyQueue) == false)
        {
            throw new InvalidOperationException("No async copy queue for device?");
        }

        if (FindQueue(queueFamilies, VkQueueFlags.Compute, VkQueueFlags.Graphics, out var computeQueue) == false)
        {
            throw new InvalidOperationException("No async compute queue for device?");
        }

        deviceInfo.Queues = new VulkanQueueInfo[(int)VulkanQueueType.Count];

        deviceInfo.Queues[(int)VulkanQueueType.Graphics] = graphicsQueue;
        deviceInfo.Queues[(int)VulkanQueueType.Copy] = copyQueue;
        deviceInfo.Queues[(int)VulkanQueueType.Compute] = computeQueue;

        return deviceInfo;
    }

    private unsafe static VkUtf8String[] EnumerateInstanceLayers()
    {
        uint count = 0;

        VkResult result = vkEnumerateInstanceLayerProperties(&count, null);

        if (result != VkResult.Success || count == 0)
        {
            return [];
        }

        VkLayerProperties[] props = new VkLayerProperties[(int)count];
        vkEnumerateInstanceLayerProperties(props).CheckResult();

        VkUtf8String[] resultExt = new VkUtf8String[count];
        for (int i = 0; i < count; i++)
        {
            fixed (byte* pLayerName = props[i].layerName)
            {
                resultExt[i] = new VkUtf8String(pLayerName);
            }
        }

        return resultExt;
    }

    private static unsafe VkUtf8String[] GetInstanceExtensions()
    {
        uint count = 0;
        VkResult result = vkEnumerateInstanceExtensionProperties(&count, null);

        if (result != VkResult.Success)
        {
            return [];
        }

        if (count == 0)
        {
            return [];
        }

        VkExtensionProperties[] props = new VkExtensionProperties[(int)count];
        vkEnumerateInstanceExtensionProperties(props);

        VkUtf8String[] extensions = new VkUtf8String[count];
        for (int i = 0; i < count; i++)
        {
            fixed (byte* pExtensionName = props[i].extensionName)
            {
                extensions[i] = new VkUtf8String(pExtensionName);
            }
        }

        return extensions;
    }

    private static bool FindQueue(ReadOnlySpan<VkQueueFamilyProperties> queueProperties, VkQueueFlags includeFlags, VkQueueFlags excludeFlags, out VulkanQueueInfo queueInfo)
    {
        uint index = 0;

        queueInfo = new VulkanQueueInfo
        {
            Index = VK_QUEUE_FAMILY_IGNORED,
            Flags = VkQueueFlags.None
        };

        foreach (var queue in queueProperties)
        {
            if((queue.queueFlags & includeFlags) != VkQueueFlags.None)
            {
                if((queue.queueFlags & excludeFlags) == VkQueueFlags.None)
                {
                    queueInfo = new VulkanQueueInfo
                    {
                        Index = index,
                        Flags = queue.queueFlags
                    };

                    return true;
                }
            }

            index++;
        }

        return false;
    }

    [UnmanagedCallersOnly]
    private static unsafe uint DebugMessengerCallback(VkDebugUtilsMessageSeverityFlagsEXT messageSeverity,
    VkDebugUtilsMessageTypeFlagsEXT messageTypes,
    VkDebugUtilsMessengerCallbackDataEXT* pCallbackData,
    void* userData)
    {
        VkUtf8String message = new VkUtf8String(pCallbackData->pMessage)!;
        if (messageTypes == VkDebugUtilsMessageTypeFlagsEXT.Validation)
        {
            if (messageSeverity == VkDebugUtilsMessageSeverityFlagsEXT.Error)
            {
                Debug.WriteLine($"[Vulkan]: Validation: {messageSeverity} - {message}");
            }
            else if (messageSeverity == VkDebugUtilsMessageSeverityFlagsEXT.Warning)
            {
                Debug.WriteLine($"[Vulkan]: Validation: {messageSeverity} - {message}");
            }

            Debug.WriteLine($"[Vulkan]: Validation: {messageSeverity} - {message}");
        }
        else
        {
            if (messageSeverity == VkDebugUtilsMessageSeverityFlagsEXT.Error)
            {
                Debug.WriteLine($"[Vulkan]: {messageSeverity} - {message}");
            }
            else if (messageSeverity == VkDebugUtilsMessageSeverityFlagsEXT.Warning)
            {
                Debug.WriteLine($"[Vulkan]: {messageSeverity} - {message}");
            }

            Debug.WriteLine($"[Vulkan]: {messageSeverity} - {message}");
        }

        Debugger.Break();

        return VK_FALSE;
    }
}
