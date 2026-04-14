// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Text;

using Vortice.Vulkan;

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

    internal static void SetDebugName(VkBuffer buffer, string name) => vkSetDebugUtilsObjectNameEXT(buffer, name, VkObjectType.Buffer);
    internal static void SetDebugName(VkImage image, string name) => vkSetDebugUtilsObjectNameEXT(image, name, VkObjectType.Image);
    internal static void SetDebugName(VkImageView imageView, string name) => vkSetDebugUtilsObjectNameEXT(imageView, name, VkObjectType.ImageView);

    internal static void SetDebugName(VkCommandBuffer commandBuffer, string name) => vkSetDebugUtilsObjectNameEXT((ulong)commandBuffer.Handle, name, VkObjectType.CommandBuffer);

    internal static void SetDebugName(VkQueue queue, string name) => vkSetDebugUtilsObjectNameEXT((ulong)queue.Handle, name, VkObjectType.Queue);

    internal static void SetDebugName(VkFence fence, string name) => vkSetDebugUtilsObjectNameEXT((ulong)fence, name, VkObjectType.Fence);

    internal static void SetDebugName(VkSemaphore semaphore, string name) => vkSetDebugUtilsObjectNameEXT((ulong)semaphore, name, VkObjectType.Semaphore);

    internal static unsafe void BeginLabel(VkCommandBuffer commandBuffer, VkDebugUtilsLabelEXT label)
    {
        _instance!.Api.vkCmdBeginDebugUtilsLabelEXT(commandBuffer, &label);
    }

    internal static unsafe void EndLabel(VkCommandBuffer commandBuffer)
    {
        _instance!.Api.vkCmdEndDebugUtilsLabelEXT(commandBuffer);
    }

    internal static unsafe void InsertMarker(VkCommandBuffer commandBuffer, VkDebugUtilsLabelEXT label)
    {
        _instance!.Api.vkCmdInsertDebugUtilsLabelEXT(commandBuffer, &label);
    }

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
