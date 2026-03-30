// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;
using System.Runtime.InteropServices;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.RHI;
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

internal struct VulkanTextureDescription
{
    public uint Width;
    public uint Height;
    public VkFormat Format;
    public VkImageUsageFlags Usage;
    public string? Name;
}

internal struct VulkanTextureState
{
    internal required VkImageLayout Layout = VkImageLayout.Undefined;
    internal required VkAccessFlags2 AccessMask = VkAccessFlags2.None;
    internal required VkPipelineStageFlags2 StageMask = VkPipelineStageFlags2.None;
    internal required uint QueueFamilyIndex = 0;

    public VulkanTextureState()
    {

    }
}

internal static class RhiTextureDescriptionExtensions
{
    internal static VulkanTextureDescription ToVulkanTextureDescription(this RhiTextureDescription rhiTextureDescription)
    {
        return new VulkanTextureDescription
        {
            Width = rhiTextureDescription.Width,
            Height = rhiTextureDescription.Height,
            Format = VkFormat.R8G8B8A8Unorm,
            Usage = rhiTextureDescription.Usage.ToVkImageUsageFlags(),
            Name = rhiTextureDescription.Name
        };
    }

    internal static VulkanTextureState ToVkTextureState(this RhiRenderTextureUsage rhiRenderTextureUsage, uint queueFamilyIndex)
    {
        return rhiRenderTextureUsage switch
        {
            RhiRenderTextureUsage.CopyDestination => new VulkanTextureState
            {
                Layout = VkImageLayout.TransferDstOptimal,
                AccessMask = VkAccessFlags2.TransferWrite,
                StageMask = VkPipelineStageFlags2.Transfer,
                QueueFamilyIndex = queueFamilyIndex
            },
            RhiRenderTextureUsage.CopySource => new VulkanTextureState
            {
                Layout = VkImageLayout.TransferSrcOptimal,
                AccessMask = VkAccessFlags2.TransferRead,
                StageMask = VkPipelineStageFlags2.Transfer,
                QueueFamilyIndex = queueFamilyIndex
            },
            RhiRenderTextureUsage.Sampler => new VulkanTextureState
            {
                Layout = VkImageLayout.ShaderReadOnlyOptimal,
                AccessMask = VkAccessFlags2.ShaderRead,
                StageMask = VkPipelineStageFlags2.FragmentShader,
                QueueFamilyIndex = queueFamilyIndex
            },
            RhiRenderTextureUsage.ColourTarget => new VulkanTextureState
            {
                Layout = VkImageLayout.ColorAttachmentOptimal,
                AccessMask = VkAccessFlags2.ColorAttachmentWrite,
                StageMask = VkPipelineStageFlags2.ColorAttachmentOutput,
                QueueFamilyIndex = queueFamilyIndex
            },
            RhiRenderTextureUsage.Present => new VulkanTextureState
            {
                Layout = VkImageLayout.PresentSrcKHR,
                AccessMask = VkAccessFlags2.None,
                StageMask = VkPipelineStageFlags2.None,
                QueueFamilyIndex = queueFamilyIndex
            },
            _ => new VulkanTextureState
            {
                Layout = VkImageLayout.Undefined,
                AccessMask = VkAccessFlags2.None,
                StageMask = VkPipelineStageFlags2.None,
                QueueFamilyIndex = queueFamilyIndex
            }
        };
    }

}

[DebuggerDisplay("VulkanTexture {Name}")]
internal class VulkanTexture: IRenderTexture, IDisposable
{
    public VkImage Image { get; private set; }
    public VkImageView ImageView { get; private set; }

    public readonly VulkanTextureDescription Description;

    public readonly byte[] Texels = [];

    private bool _disposed;
    private readonly VulkanDevice _device;

    public string Name { get => Description.Name!; set => throw new NotImplementedException(); }

    public nint Handle => throw new NotImplementedException();

    public uint Width => Description.Width;

    public uint Height => Description.Height;

    public VulkanTextureState State;

    private VulkanMemoryAllocation _allocation;

    private bool _ownsImage = true;

    private RhiVkImageInterop _imageInterop;

    private static uint _debugTextureCount = uint.MaxValue;

    internal unsafe VulkanTexture(VulkanDevice device, in VulkanTextureDescription textureDescription, VkImage existingImage = default)
    {
        _device = device;
        Description = textureDescription;

        uint debugTextureCount = ++_debugTextureCount;

        Description.Name = textureDescription.Name ?? $"Texture{debugTextureCount}";

        if(existingImage.IsNotNull)
        {
            Image = existingImage;
            _ownsImage = false;

            FinaliseCommonSetup();

            return;
        }

        Span<uint> sharedQueueFamilyIndices =
        [
            _device.PresentQueue.FamilyIndex,
            _device.CopyQueue.FamilyIndex,
        ];

        VkImage image;

       Description.Usage = VkImageUsageFlags.TransferDst | VkImageUsageFlags.TransferSrc | VkImageUsageFlags.Sampled;
       Description.Usage |= textureDescription.Usage;

        // TODO: Not all textures are created by us, e.g. swapchain. So this currently results in an extra one that gets
        // orphaned.
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
                format = textureDescription.Format,
                tiling = VkImageTiling.Optimal,
                initialLayout = VkImageLayout.Undefined,
                usage = Description.Usage,
                samples = VkSampleCountFlags.Count1,
                sharingMode = VkSharingMode.Concurrent,
                queueFamilyIndexCount = 2,
                pQueueFamilyIndices = ptr,
            };

            _device.Api.vkCreateImage(_device.Handle, &imageCreateInfo, out image);
        }

        UOEDebug.Assert(image != VkImage.Null);

        Image = image;

        _device.Api.vkGetImageMemoryRequirements(_device.Handle, image, out var memoryRequirements);

        _device.MemoryManager.AllocateImageMemory(VkMemoryPropertyFlags.DeviceLocal, memoryRequirements, out _allocation);

        _allocation.BindImage(_device, image);

        const int bytesPerTexel = 4;

        Texels = new byte[Width * Height * bytesPerTexel];

        FinaliseCommonSetup();

    }

    internal VulkanTexture(VulkanDevice device, in RhiTextureDescription description)
        : this(device, description.ToVulkanTextureDescription())
    {
    }

    public Span<T> GetTexelsAs<T>() where T : unmanaged
    {
        return MemoryMarshal.Cast<byte, T>(Texels.AsSpan());
    }

    public void Upload() => Upload(0, 0, Width, Height);

    public void Upload(uint x = 0, uint y = 0, uint w = 0, uint h = 0)
    {
        // Lazy bad lock!
        lock (_device.ImmediateUploadLock)
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
                var dst = bufferLock.buffer.GetSpan();

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

            var commandBuffer = _device.GraphicsQueue.UploadContext.GetCommandBuffer();

            commandBuffer.BeginRecording();
            commandBuffer.CmdCopyBufferToImage(bufferLock.vkBuffer, this, bufferImageCopy);
            commandBuffer.EnsureState(this, RhiRenderTextureUsage.Sampler);
            commandBuffer.EndRecording();

            _device.GraphicsQueue.UploadContext.Submit();
            _device.GraphicsQueue.UploadContext.WaitForUpload();

            _device.StagingBuffer.ReleaseBuffer(bufferLock);
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    public void GetFeature<T>(out T? feature) where T : IRhiTextureInterop
    {
        if(typeof(T) == typeof(RhiVkImageInterop))
        {
            var imageInterop = new RhiVkImageInterop()
            {
                Handle = (ulong)Image,
                Format = (uint)Description.Format,
                SharingMode = (uint)VkSharingMode.Exclusive,
                ImageUsageFlags = (uint)Description.Usage,
                ImageTiling = (uint)VkImageTiling.Optimal,
                ImageLayout = (uint)State.Layout
            };

            feature = (T)(object)imageInterop;

            return;
        }

        throw new InvalidOperationException($"Unsupported texture interop feature: {typeof(T).Name}");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        
        if(_ownsImage)
        {
            _device.Api.vkDestroyImage(_device.Handle, Image);
            Image = VkImage.Null;

            _device.MemoryManager.Free(_allocation, true);
        }

         _device.Api.vkDestroyImageView(_device.Handle, ImageView);

         ImageView = VkImageView.Null;

         _disposed = true;
    }

    ~VulkanTexture()
    {
        Dispose(disposing: false);
    }

    private void FinaliseCommonSetup()
    {
        CreateImageView();

        _imageInterop.Handle = (ulong)Image;
        _imageInterop.Format = (uint)Description.Format;
        _imageInterop.SharingMode = (uint)VkSharingMode.Exclusive;
        _imageInterop.ImageUsageFlags = (uint)Description.Usage;
        _imageInterop.ImageTiling = (uint)VkImageTiling.Optimal;
        _imageInterop.ImageLayout = (uint)VkImageLayout.ColorAttachmentOptimal;

        VulkanDebug.SetDebugName(Image, Description.Name!);
    }

    private unsafe void CreateImageView()
    {
        var viewCreateInfo = new VkImageViewCreateInfo(Image, VkImageViewType.Image2D, Description.Format,
            VkComponentMapping.Rgba, new VkImageSubresourceRange(VkImageAspectFlags.Color, 0, 1, 0, 1));

        _device.Api.vkCreateImageView(_device.Handle, &viewCreateInfo, null, out var imageView).CheckResult();

        ImageView = imageView;
    }
}
