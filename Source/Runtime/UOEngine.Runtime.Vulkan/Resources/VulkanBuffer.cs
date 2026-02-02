// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Runtime.InteropServices;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.RHI;
using Vortice.DXGI;
using Vortice.Vulkan;

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
    public VkBuffer Handle => GetAllocation().Buffer;

    private readonly VulkanDevice _device;

    private readonly RhiBufferDescription Description;

    private VulkanMemoryAllocation? _allocation;

    private VulkanMemoryAllocation GetAllocation() => _allocation ?? throw new InvalidOperationException("Allocation is missing.");

    private bool _disposed;

    private int _lockCount = 0;

    private readonly VkBufferUsageFlags _usageFlags;

    internal bool IsDynamic => ((Description.Usage & RhiBufferUsageFlags.Dynamic) != 0);
    internal bool IsStatic => ((Description.Usage & RhiBufferUsageFlags.Static) != 0);

    enum LockStatus
    {
        Unlocked,
        Locked
    }

    private LockStatus _lockStatus = LockStatus.Unlocked;

    public void SetData<T>(ReadOnlySpan<T> data) where T : unmanaged
    {
        var dataBytes = MemoryMarshal.Cast<T, byte>(data);

        if (IsDynamic)
        {
            var bytes = Lock();

            dataBytes.CopyTo(bytes);

            Unlock();

            return;
        }

        Upload(dataBytes);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    internal VulkanBuffer(VulkanDevice device, in RhiBufferDescription bufferDescription)
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

        _usageFlags = usage;

        AllocateMemory(out var allocation);

        _allocation = allocation;
    }

    internal Span<byte> Lock()
    {
        UOEDebug.Assert(_lockStatus == LockStatus.Unlocked);

        _lockStatus = LockStatus.Locked;

        bool firstLock = _lockCount == 0;
        _lockCount++;

        if(IsDynamic && firstLock)
        {

        }
        else if(IsStatic)
        {
            UOEDebug.NotImplemented();
        }
        else
        {
            AllocateMemory(out var allocation);

            _device.MemoryManager.Free(GetAllocation(), true);

            _allocation = allocation;
        }

        return GetAllocation().Map(_device);
    }

    internal void Unlock()
    {
        UOEDebug.Assert(_lockStatus == LockStatus.Locked);

        _lockStatus = LockStatus.Unlocked;

    }

    internal unsafe void Upload(ReadOnlySpan<byte> data)
    {
        UOEDebug.Assert((Description.Usage | RhiBufferUsageFlags.Dynamic) == 0);

        var bufferLock = _device.StagingBuffer.AcquireBuffer(Description.Size);

        data.CopyTo(bufferLock.buffer);

        var commandBuffer = _device.GraphicsQueue.CreateCommandBuffer();

        VkBufferCopy bufferCopy = new()
        {
            dstOffset = 0,
            size = Description.Size,
            srcOffset = 0
        };

        commandBuffer.CmdCopyBuffer(bufferLock.vkBuffer, Handle, bufferCopy);

        _device.GraphicsQueue.Submit(commandBuffer);
        _device.WaitForGpuIdle();

        _device.StagingBuffer.ReleaseBuffer(bufferLock);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
            }

            _device.MemoryManager.Free(GetAllocation(), true);

            _allocation = null;

            _disposed = true;
        }
    }

    ~VulkanBuffer()
    {
        Dispose(disposing: false);
    }

    private void AllocateMemory(out VulkanMemoryAllocation allocation)
    {
        VkMemoryPropertyFlags memoryPropertyFlags = VkMemoryPropertyFlags.None;

        if (IsDynamic)
        {
            memoryPropertyFlags |= (VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent);
        }
        else
        {
            memoryPropertyFlags |= VkMemoryPropertyFlags.DeviceLocal;
        }

        _device.MemoryManager.AllocateBuffer(Description.Size, _usageFlags, memoryPropertyFlags, out allocation);

    }
}
