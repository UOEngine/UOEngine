// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.RHI;
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

public struct VulkanTextureDescription
{
    public uint Width;
    public uint Height;
    public VkFormat Format;
}

internal class VulkanTexture: IRenderTexture, IDisposable
{
    public VkImage Image { get; private set; }
    private VkImageView _imageView;

    private readonly VulkanDevice _device;
    public readonly VulkanTextureDescription Description;

    public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public nint Handle => throw new NotImplementedException();

    public uint Width => Description.Width;

    public uint Height => Description.Height;

    internal VulkanTexture(VulkanDevice device, in VulkanTextureDescription textureDescription)
    {
        _device = device;
        Description = textureDescription;
    }

    internal VulkanTexture(VulkanDevice device, in RhiTextureDescription description)
    {
        _device = device;
    }

    public unsafe void InitFromExistingResource(VkImage image)
    {
        Image = image;

        CreateImageView();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Span<T> GetTexelsAs<T>() where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public void Upload()
    {
        throw new NotImplementedException();
    }

    public void Upload(uint x = 0, uint y = 0, uint w = 0, uint h = 0)
    {
        throw new NotImplementedException();
    }

    private unsafe void CreateImageView()
    {
        var viewCreateInfo = new VkImageViewCreateInfo(Image, VkImageViewType.Image2D, Description.Format,
            VkComponentMapping.Rgba, new VkImageSubresourceRange(VkImageAspectFlags.Color, 0, 1, 0, 1));

        _device.Api.vkCreateImageView(_device.Handle, &viewCreateInfo, null, out _imageView).CheckResult();
    }
}
