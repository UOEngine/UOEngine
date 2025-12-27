// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

public struct VulkanTextureDescription
{
    public uint Width;
    public uint Height;
    public VkFormat Format;
}

internal class VulkanTexture: IDisposable
{
    private VkImage _image;
    private VkImageView _imageView;

    private readonly VulkanDevice _device;
    public readonly VulkanTextureDescription Description;

    public VulkanTexture(VulkanDevice device, in VulkanTextureDescription textureDescription)
    {
        _device = device;
        Description = textureDescription;
    }

    public unsafe void InitFromExistingResource(VkImage image)
    {
        _image = image;

        var viewCreateInfo = new VkImageViewCreateInfo( image, VkImageViewType.Image2D, Description.Format, 
            VkComponentMapping.Rgba, new VkImageSubresourceRange(VkImageAspectFlags.Color, 0, 1, 0, 1));

        _device.Api.vkCreateImageView(_device.Handle, &viewCreateInfo, null, out _imageView).CheckResult();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
