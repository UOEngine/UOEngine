// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Xml.Linq;
using UOEngine.Runtime.Core;
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

internal class VulkanSwapchain
{
    public VkSwapchainKHR Handle => _handle;

    private VkSurfaceKHR _surface;
    private nint _windowHandle;
    private readonly VulkanInstance _instance;
    private readonly VulkanDevice _device;

    private VkSwapchainKHR _handle;

    private VulkanTexture[] _backbuffer = [];

    public VulkanSwapchain(VulkanInstance instance, VulkanDevice device)
    {
        _instance = instance;
        _device = device;
    }

    public unsafe void Create(nint windowHandle)
    {
        _windowHandle = windowHandle;
        VkSurfaceKHR* surface = default;

        if (UOEngineSdl3.SDL_Vulkan_CreateSurface(windowHandle, _instance.Instance, null, &surface) == false)
        {
            throw new Exception("SDL: failed to create vulkan surface");
        }

        _surface = new VkSurfaceKHR((ulong)new IntPtr(surface).ToInt64());

        _instance.Api.vkGetPhysicalDeviceSurfaceSupportKHR(_device.PhysicalDeviceHandle, _device.PresentQueue.FamilyIndex, _surface, out var supported).CheckResult();

        if(supported == false)
        {
            throw new Exception("Surface does not support presentation");
        }

        CreateSwapchain();
    }

    private unsafe void CreateSwapchain()
    {
        SwapChainSupportDetails swapChainSupport = QuerySwapChainSupport();

        VkSurfaceFormatKHR surfaceFormat = ChooseSwapSurfaceFormat(swapChainSupport.Formats);
        VkPresentModeKHR presentMode = ChooseSwapPresentMode(swapChainSupport.PresentModes);
        VkExtent2D Extent = ChooseSwapExtent(swapChainSupport.Capabilities);

        uint imageCount = 2;

        VkSwapchainCreateInfoKHR createInfo = new()
        {
            surface = _surface,
            minImageCount = imageCount,
            imageFormat = surfaceFormat.format,
            imageColorSpace = surfaceFormat.colorSpace,
            imageExtent = Extent,
            imageArrayLayers = 1,
            imageUsage = VkImageUsageFlags.ColorAttachment,
            imageSharingMode = VkSharingMode.Exclusive,
            preTransform = swapChainSupport.Capabilities.currentTransform,
            compositeAlpha = VkCompositeAlphaFlagsKHR.Opaque,
            presentMode = presentMode,
            clipped = true,
            oldSwapchain = VkSwapchainKHR.Null
        };

        _device.Api.vkCreateSwapchainKHR(_device.Handle,  &createInfo, out _handle).CheckResult();

        // Grab created swapchain images

        _device.Api.vkGetSwapchainImagesKHR(_device.Handle, Handle, out uint swapChainImageCount).CheckResult();

        Span<VkImage> swapChainImages = stackalloc VkImage[(int)swapChainImageCount];

        _device.Api.vkGetSwapchainImagesKHR(_device.Handle, Handle, swapChainImages).CheckResult();

        _backbuffer = new VulkanTexture[swapChainImageCount];

        for(int i = 0;  i < swapChainImageCount; i++)
        {
            _backbuffer[i] = new VulkanTexture();
        }
    }

    private static VkPresentModeKHR ChooseSwapPresentMode(ReadOnlySpan<VkPresentModeKHR> availablePresentModes)
    {
        foreach (VkPresentModeKHR availablePresentMode in availablePresentModes)
        {
            if (availablePresentMode == VkPresentModeKHR.Mailbox)
            {
                return availablePresentMode;
            }
        }

        return VkPresentModeKHR.Fifo;
    }

    public ref struct SwapChainSupportDetails
    {
        public VkSurfaceCapabilitiesKHR Capabilities;
        public Span<VkSurfaceFormatKHR> Formats;
        public Span<VkPresentModeKHR> PresentModes;
    }

    public SwapChainSupportDetails QuerySwapChainSupport()
    {
        SwapChainSupportDetails details = new();
        _instance.Api.vkGetPhysicalDeviceSurfaceCapabilitiesKHR(_device.PhysicalDeviceHandle, _surface, out details.Capabilities).CheckResult();

        _instance.Api.vkGetPhysicalDeviceSurfaceFormatsKHR(_device.PhysicalDeviceHandle, _surface, out uint surfaceFormatCount).CheckResult();
        details.Formats = new VkSurfaceFormatKHR[surfaceFormatCount];
        _instance.Api.vkGetPhysicalDeviceSurfaceFormatsKHR(_device.PhysicalDeviceHandle, _surface, details.Formats).CheckResult();

        _instance.Api.vkGetPhysicalDeviceSurfacePresentModesKHR(_device.PhysicalDeviceHandle, _surface, out uint presentModeCount).CheckResult();
        details.PresentModes = new VkPresentModeKHR[presentModeCount];
        _instance.Api.vkGetPhysicalDeviceSurfacePresentModesKHR(_device.PhysicalDeviceHandle, _surface, details.PresentModes).CheckResult();

        return details;
    }

    private VkExtent2D ChooseSwapExtent(VkSurfaceCapabilitiesKHR capabilities)
    {
        if (capabilities.currentExtent.width > 0)
        {
            return capabilities.currentExtent;
        }
        else
        {
            VkExtent2D actualExtent = new VkExtent2D(1, 1);

            actualExtent = new VkExtent2D(
                Math.Max(capabilities.minImageExtent.width, Math.Min(capabilities.maxImageExtent.width, actualExtent.width)),
                Math.Max(capabilities.minImageExtent.height, Math.Min(capabilities.maxImageExtent.height, actualExtent.height))
                );

            return actualExtent;
        }
    }
    private static VkSurfaceFormatKHR ChooseSwapSurfaceFormat(ReadOnlySpan<VkSurfaceFormatKHR> availableFormats)
    {
        // If the surface format list only includes one entry with VK_FORMAT_UNDEFINED,
        // there is no preferred format, so we assume VK_FORMAT_B8G8R8A8_UNORM
        if ((availableFormats.Length == 1) && (availableFormats[0].format == VkFormat.Undefined))
        {
            return new VkSurfaceFormatKHR(VkFormat.B8G8R8A8Unorm, availableFormats[0].colorSpace);
        }

        // iterate over the list of available surface format and
        // check for the presence of VK_FORMAT_B8G8R8A8_UNORM
        foreach (VkSurfaceFormatKHR availableFormat in availableFormats)
        {
            if (availableFormat.format == VkFormat.B8G8R8A8Unorm)
            {
                return availableFormat;
            }
        }

        return availableFormats[0];
    }
}
