// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Runtime.InteropServices;

using Vortice.Vulkan;

using UOEngine.Runtime.Core;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Vulkan;

internal static class RhiBufferUsageExtensions
{
    internal static VkBufferUsageFlags ToVkBufferUsageFlags(this RhiBufferUsageFlags flags)
    {
        VkBufferUsageFlags vkFlags = VkBufferUsageFlags.None;

       if((flags & RhiBufferUsageFlags.Vertex) != 0)
        {
            vkFlags |= VkBufferUsageFlags.VertexBuffer;
        }

        return vkFlags;
    }
}

internal class VulkanBuffer: IRhiBuffer, IDisposable
{
    public readonly VkBuffer Handle;

    private readonly VulkanDevice _device;
    private readonly VkDeviceMemory _gpuBufferMemory;

    private readonly RhiBufferDescription Description;

    private VulkanAllocation _mappedMemory;

    private ulong _allocationSize;

    internal bool IsDynamic => ((Description.Usage & RhiBufferUsageFlags.Dynamic) != 0);

    public void SetData<T>(ReadOnlySpan<T> data) where T : unmanaged
    {
        var dataBytes = MemoryMarshal.Cast<T, byte>(data);

        if (IsDynamic)
        {
            unsafe
            {
                dataBytes.CopyTo(_mappedMemory.AsSpan());
            }

            return;
        }

        Upload(dataBytes);
    }

    internal unsafe VulkanBuffer(VulkanDevice device, in RhiBufferDescription bufferDescription)
    {
        _device = device;
        Description = bufferDescription;

        VkBufferUsageFlags usage = VkBufferUsageFlags.None;

        if ((bufferDescription.Usage & RhiBufferUsageFlags.Vertex) != 0)
        {
            usage |= VkBufferUsageFlags.VertexBuffer;
        }

        if ((bufferDescription.Usage & RhiBufferUsageFlags.Index) != 0)
        {
            usage |= VkBufferUsageFlags.IndexBuffer;
        }

        bool isDynamic = ((bufferDescription.Usage & RhiBufferUsageFlags.Dynamic) != 0);

        VkBufferCreateInfo bufferCreateInfo = new()
        {
            size = bufferDescription.Size,
            usage = usage
        };

        device.Api.vkCreateBuffer(device.Handle, &bufferCreateInfo, out Handle);

        device.Api.vkGetBufferMemoryRequirements(device.Handle, Handle, out var memoryRequirements);

        VkMemoryPropertyFlags memoryPropertyFlags = VkMemoryPropertyFlags.None;

        if ((bufferDescription.Usage & RhiBufferUsageFlags.Dynamic) != 0)
        {
            memoryPropertyFlags |= (VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent);
        }

        _allocationSize = memoryRequirements.size;

        _mappedMemory = new VulkanAllocation()
        {
            Size = memoryRequirements.size
        };

         VkMemoryAllocateInfo memoryAllocateInfo = new()
        {
            allocationSize = memoryRequirements.size,
            memoryTypeIndex = device.GetMemoryTypeIndex(memoryRequirements.memoryTypeBits, memoryPropertyFlags),
        };

        device.Api.vkAllocateMemory(device.Handle, &memoryAllocateInfo, out _gpuBufferMemory);

        device.Api.vkBindBufferMemory(device.Handle, Handle, _gpuBufferMemory, 0);

        if (isDynamic)
        {
            Map();
        }
    }

    public unsafe void Dispose()
    {
        _device.Api.vkDestroyBuffer(_device.Handle, Handle);
        _device.Api.vkFreeMemory(_device.Handle, _gpuBufferMemory, null);
    }

    internal unsafe void Upload(ReadOnlySpan<byte> data)
    {
        UOEDebug.Assert((Description.Usage | RhiBufferUsageFlags.Dynamic) == 0);

        var bufferLock = _device.StagingBuffer.AcquireBuffer((uint)_allocationSize);

        data.CopyTo(bufferLock.buffer);

        var commandBuffer = _device.GraphicsQueue.CreateCommandBuffer();

        VkBufferCopy bufferCopy = new()
        {
            dstOffset = 0,
            size = _allocationSize,
            srcOffset = 0
        };

        commandBuffer.CmdCopyBuffer(bufferLock.vkBuffer, Handle, bufferCopy);

        _device.GraphicsQueue.Submit(commandBuffer);
        _device.WaitForGpuIdle();

        _device.StagingBuffer.ReleaseBuffer(bufferLock);
    }

    private unsafe void Map()
    {
        void* mapped;

        _device.Api.vkMapMemory(_device.Handle, _gpuBufferMemory, 0, Description.Size, 0, &mapped);

        _mappedMemory.Ptr = (byte*)mapped;
    }

    private unsafe void Unmap()
    {
        _device.Api.vkUnmapMemory(_device.Handle, _gpuBufferMemory);
        _mappedMemory.Ptr = (byte*)0;
    }
}
