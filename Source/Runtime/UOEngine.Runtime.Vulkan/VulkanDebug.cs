// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Text;

using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace UOEngine.Runtime.Vulkan;

internal static class VulkanDebug
{
    private static VulkanDevice? _device;
    private static VulkanInstance? _instance;

    internal static void Init(VulkanDevice device,  VulkanInstance instance)
    {
        _device = device;
        _instance = instance;
    }

    internal static unsafe void SetDebugName(VkBuffer buffer, string name) => vkSetDebugUtilsObjectNameEXT(buffer, name, VkObjectType.Buffer);
    internal static unsafe void SetDebugName(VkImage image, string name) => vkSetDebugUtilsObjectNameEXT(image, name, VkObjectType.Image);
    internal static unsafe void SetDebugName(VkImageView imageView, string name) => vkSetDebugUtilsObjectNameEXT(imageView, name, VkObjectType.ImageView);


    private static unsafe void vkSetDebugUtilsObjectNameEXT(ulong objectHandle, string name, VkObjectType objectType)
    {
        VkUtf8ReadOnlyString pName = Encoding.UTF8.GetBytes(name);

        VkDebugUtilsObjectNameInfoEXT nameInfo = new()
        {
            objectType = objectType,
            objectHandle = objectHandle,
            pObjectName = pName
        };

        _instance!.Api.vkSetDebugUtilsObjectNameEXT(_device!.Handle, &nameInfo);
    }
}
