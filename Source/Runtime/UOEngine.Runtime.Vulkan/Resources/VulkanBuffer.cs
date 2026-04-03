// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
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
    public VkBuffer Handle => GetAllocation().Buffer;

    private readonly VulkanDevice _device;

    public RhiBufferDescription Description { get; private set;  }

    private VulkanMemoryAllocation? _allocation;

    private VulkanMemoryAllocation GetAllocation() => _allocation ?? throw new InvalidOperationException("Allocation is missing.");

    private bool _disposed;

    private int _lockCount = 0;

    private readonly VkBufferUsageFlags _usageFlags;

    internal bool IsDynamic => ((Description.Usage & RhiBufferUsageFlags.Dynamic) != 0);
    internal bool IsStatic => ((Description.Usage & RhiBufferUsageFlags.Static) != 0) || (IsDynamic == false);

    enum LockStatus
    {
        Unlocked,
        Locked,
        PersistentMapping
    }

    private LockStatus _lockStatus = LockStatus.Unlocked;

    private VulkanStagingBufferLock _pendingStagingBuffer;

    public void SetData<T>(ReadOnlySpan<T> data) where T : unmanaged
    {
        var dataBytes = MemoryMarshal.Cast<T, byte>(data);

        var bytes = Lock();

        dataBytes.CopyTo(bytes);

        Unlock();
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

        //_fence = new VulkanFence(_device);
    }

    public Span<byte> Lock() => Lock(Description.Size, 0);

    public Span<byte> Lock(uint size, uint offset)
    {
        UOEDebug.Assert(_lockStatus == LockStatus.Unlocked);

        Span<byte> data = null;
        
        bool firstLock = _lockCount == 0;
        _lockCount++;
        
        if (IsDynamic && firstLock)
        {
            data = GetAllocation().Map(_device);
        
            _lockStatus = LockStatus.PersistentMapping;
        
        }
        else if (IsStatic)
        {
            _pendingStagingBuffer = _device.StagingBuffer.AcquireBuffer(size);
        
            data = _pendingStagingBuffer.buffer.GetSpan();
        
            _lockStatus = LockStatus.Locked;
        }
        else
        {
            AllocateMemory(out var allocation);
        
            _device.MemoryManager.Free(GetAllocation(), true);
        
            _allocation = allocation;
        
            data = GetAllocation().Map(_device);
        
            _lockStatus = LockStatus.PersistentMapping;
        
        }
        
        return data;
    }

    public void Unlock()
    {
        UOEDebug.Assert(_lockStatus != LockStatus.Unlocked);

        if (_lockStatus == LockStatus.Locked)
        {

            VkBufferCopy bufferCopy = new()
            {
                srcOffset = _pendingStagingBuffer.Offset,
                dstOffset = GetAllocation().Offset,
                size = _pendingStagingBuffer.buffer.Length
            };

             _device.GraphicsQueue.UploadContext.QueueBufferUpload(_pendingStagingBuffer.vkBuffer, bufferCopy, Handle);

            //commandBuffer.BeginRecording();
            //commandBuffer.CmdCopyBuffer(_pendingStagingBuffer.vkBuffer, Handle, bufferCopy);

            //// Barrier?

            //commandBuffer.EndRecording();


            //_device.GraphicsQueue.UploadContext.Submit();
            //_device.GraphicsQueue.UploadContext.WaitForUpload();


            //_device.StagingBuffer.ReleaseBuffer(_pendingStagingBuffer);
        }

        _lockStatus = LockStatus.Unlocked;
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

        VkBufferUsageFlags usageFlags = _usageFlags;

        if (IsDynamic)
        {
            memoryPropertyFlags |= (VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent);
        }
        else
        {
            memoryPropertyFlags |= VkMemoryPropertyFlags.DeviceLocal;

            usageFlags |= VkBufferUsageFlags.TransferDst;
        }

        _device.MemoryManager.AllocateBuffer(Description.Size, usageFlags, memoryPropertyFlags, out allocation);
    }
}
