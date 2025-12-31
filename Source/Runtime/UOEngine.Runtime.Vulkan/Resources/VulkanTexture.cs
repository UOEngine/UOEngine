// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Runtime.InteropServices;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.RHI;
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

public struct VulkanTextureDescription
{
    public uint Width;
    public uint Height;
    public VkFormat Format;
}

internal static class RhiTextureDescriptionExtensions
{
    internal static VulkanTextureDescription ToVulkanTextureDescription(this RhiTextureDescription rhiTextureDescription)
    {
        return new VulkanTextureDescription
        {
            Width = rhiTextureDescription.Width,
            Height = rhiTextureDescription.Height,
            Format = VkFormat.R8G8B8A8Unorm
        };
    }
}

internal class VulkanTexture: IRenderTexture, IDisposable
{
    public VkImage Image { get; private set; }
    public VkImageView ImageView { get; private set; }

    public readonly VulkanTextureDescription Description;

    public readonly byte[] Texels = [];

    private VkImageView _imageView;

    private readonly VulkanDevice _device;


    public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public nint Handle => throw new NotImplementedException();

    public uint Width => Description.Width;

    public uint Height => Description.Height;

    private readonly VkDeviceMemory _deviceMemory;

    internal unsafe VulkanTexture(VulkanDevice device, in VulkanTextureDescription textureDescription)
    {
        _device = device;
        Description = textureDescription;

        Span<uint> sharedQueueFamilyIndices = stackalloc uint[2];

        sharedQueueFamilyIndices[0] = _device.PresentQueue.FamilyIndex;
        sharedQueueFamilyIndices[1] = _device.CopyQueue.FamilyIndex;

        VkImage image;

        fixed (uint* ptr = sharedQueueFamilyIndices)
        {

            VkImageCreateInfo imageCreateInfo = new()
            {
                imageType = VkImageType.Image2D,
                extent = new()
                {
                    width = Width,
                    height = Height,
                    depth = 1
                },
                mipLevels = 1,
                arrayLayers = 1,
                format = VkFormat.R8G8B8A8Unorm,
                tiling = VkImageTiling.Optimal,
                initialLayout = VkImageLayout.Undefined,
                usage = VkImageUsageFlags.TransferDst | VkImageUsageFlags.Sampled,
                samples = VkSampleCountFlags.Count1,
                sharingMode = VkSharingMode.Concurrent,
                queueFamilyIndexCount = 2,
                pQueueFamilyIndices = ptr
            };

            _device.Api.vkCreateImage(_device.Handle, &imageCreateInfo, out image);
        }

        UOEDebug.Assert(image != VkImage.Null);

        Image = image;

        _device.Api.vkGetImageMemoryRequirements(_device.Handle, image, out var memoryRequirements);

        VkMemoryAllocateInfo memoryAllocateInfo = new()
        {
            allocationSize = memoryRequirements.size,
            memoryTypeIndex = _device.GetMemoryTypeIndex(memoryRequirements.memoryTypeBits, VkMemoryPropertyFlags.DeviceLocal)
        };

        _device.Api.vkAllocateMemory(_device.Handle, &memoryAllocateInfo, out var deviceMemory);

        _deviceMemory = deviceMemory;

        _device.Api.vkBindImageMemory(_device.Handle, Image, deviceMemory, 0);

        const int bytesPerTexel = 4;

        Texels = new byte[Width * Height * bytesPerTexel];

    }

    internal VulkanTexture(VulkanDevice device, in RhiTextureDescription description)
        : this(device, description.ToVulkanTextureDescription())
    {
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
        return MemoryMarshal.Cast<byte, T>(Texels.AsSpan());
    }

    public void Upload() => Upload(0, 0, Width, Height);

    public void Upload(uint x = 0, uint y = 0, uint w = 0, uint h = 0)
    {
        // Assuming RGBA for now.
        const uint bytesPerTexel = 4;

        uint size = w * h * bytesPerTexel;

        var bufferLock = _device.StagingBuffer.AcquireBuffer(size);

        var src = new ReadOnlySpan<byte>(Texels);

        int srcRowBytes = (int)(w * bytesPerTexel);
        uint srcRowPitch = Width * bytesPerTexel;

        unsafe
        {
            var dst = bufferLock.buffer;

            for (uint row = 0; row < h; row++)
            {
                var srcRow = src.Slice((int)((y + row) * srcRowPitch + x * bytesPerTexel), srcRowBytes);
                var dstRow = dst.Slice((int)(row * srcRowBytes), srcRowBytes);

                srcRow.CopyTo(dstRow);
            }
        }

        VkBufferImageCopy bufferImageCopy = new()
        {
            bufferOffset = bufferLock.Offset,
            bufferRowLength = 0,
            bufferImageHeight = 0,
            imageSubresource = new()
            {
                aspectMask = VkImageAspectFlags.Color,
                mipLevel = 0,
                baseArrayLayer = 0,
                layerCount = 1
            },
            imageOffset = new()
            {
                x = (int)x,
                y = (int)y,
                z = 0
            },
            imageExtent = new()
            {
                width = w,
                height = h,
                depth = 1
            }
        };

        var commandBuffer = _device.GraphicsQueue.CreateCommandBuffer();

        commandBuffer.TransitionImageLayout(Image, VkImageLayout.Undefined, VkImageLayout.TransferDstOptimal);
        commandBuffer.CmdCopyBufferToImage(bufferLock.vkBuffer, Image, bufferImageCopy);
        commandBuffer.TransitionImageLayout(Image, VkImageLayout.TransferDstOptimal, VkImageLayout.ShaderReadOnlyOptimal);
        commandBuffer.EndRecording();


        _device.GraphicsQueue.Submit(commandBuffer);
        _device.WaitForGpuIdle();

        _device.StagingBuffer.ReleaseBuffer(bufferLock);
    }

    private unsafe void CreateImageView()
    {
        var viewCreateInfo = new VkImageViewCreateInfo(Image, VkImageViewType.Image2D, Description.Format,
            VkComponentMapping.Rgba, new VkImageSubresourceRange(VkImageAspectFlags.Color, 0, 1, 0, 1));

        _device.Api.vkCreateImageView(_device.Handle, &viewCreateInfo, null, out var imageView).CheckResult();

        ImageView = imageView;
    }
}
