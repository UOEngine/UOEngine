// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

using Vortice.Vulkan;

using UOEngine.Runtime.Core;
using UOEngine.Runtime.RHI;
using System.Diagnostics;

namespace UOEngine.Runtime.Vulkan;

internal class VulkanUploadContext
{
    internal bool InFlight { get; private set; }

    private readonly VulkanFence _fence;

    private readonly  VulkanCommandBufferPool Pool;

    private readonly VulkanCommandBuffer _commandBuffer;
    private readonly VulkanDevice _device;

    enum UploadResourceType
    {
        Texture,
        Buffer
    }

    [DebuggerDisplay("Upload {Type.ToString()} Operation")]
    private struct UploadOperation
    {
        internal UploadResourceType Type;
        internal VulkanTexture Texture;
        internal VkBufferImageCopy ImageCopy;
        internal VkBuffer Source;
        internal VkBufferCopy BufferCopy;
        internal VkBuffer Destination;
    }

    private ConcurrentQueue<UploadOperation> _pendingUploads = new();

    private List<VulkanTexture> _usedTextures = [];

    internal VulkanUploadContext(VulkanDevice device, uint queueFamilyIndex)
    {
        _device = device;
        Pool = new VulkanCommandBufferPool(device, queueFamilyIndex);
        _commandBuffer = Pool.Create();
        _fence = new VulkanFence(device, false);
    }

    internal void QueueBufferUpload(VkBuffer source, VkBufferCopy bufferCopy, VkBuffer destination)
    {
        if(UOEThread.IsMainThread)
        {
            Pool.Reset();

            _commandBuffer.BeginRecording();
            _commandBuffer.CmdCopyBuffer(source, destination, bufferCopy);
            _commandBuffer.EndRecording();

            _device.GraphicsQueue.Submit(_commandBuffer, _fence);
            _fence.WaitForThenReset();
            _device.StagingBuffer.HackBufferFree();

            return;
        }

        _pendingUploads.Enqueue(new UploadOperation
        {
            Type = UploadResourceType.Buffer,
            Source = source,
            BufferCopy = bufferCopy,
            Destination = destination
        });
    }

    internal void QueueUpload(VulkanTexture texture, VkBufferImageCopy imageCopy, VkBuffer source)
    {
        if (UOEThread.IsMainThread)
        {
            Pool.Reset();

            _commandBuffer.BeginRecording();
            _commandBuffer.CmdCopyBufferToImage(source, texture, imageCopy);
            _commandBuffer.EnsureState(texture, RhiRenderTextureUsage.Sampler);
            _commandBuffer.EndRecording();

            _device.GraphicsQueue.Submit(_commandBuffer, _fence);
            _fence.WaitForThenReset();

            texture.Ready();
            _device.StagingBuffer.HackBufferFree();

            return;
        }

        _pendingUploads.Enqueue(new UploadOperation
        {
            Type = UploadResourceType.Texture,
            Texture = texture,
            ImageCopy = imageCopy,
            Source = source
        });
    }

    internal void FlushQueuedUploads()
    {
        // Flush on main only!
        UOEDebug.Assert(UOEThread.IsMainThread);

        if (_pendingUploads.Count == 0)
        {
            return;
        }

        Pool.Reset();

        _commandBuffer.BeginRecording();

        while(_pendingUploads.TryDequeue(out var uploadOperation))
        {
            switch (uploadOperation.Type)
            {
                case UploadResourceType.Buffer:
                    {
                        _commandBuffer.CmdCopyBuffer(uploadOperation.Source, uploadOperation.Destination, uploadOperation.BufferCopy);

                        break;
                    }
                case UploadResourceType.Texture:
                    {
                        _commandBuffer.CmdCopyBufferToImage(uploadOperation.Source, uploadOperation.Texture, uploadOperation.ImageCopy);
                        _commandBuffer.EnsureState(uploadOperation.Texture, RhiRenderTextureUsage.Sampler);

                        _usedTextures.Add(uploadOperation.Texture);

                        break;
                    }
                default:
                    throw new SwitchExpressionException();
            };

        }

        _commandBuffer.EndRecording();

        _device.GraphicsQueue.Submit(_commandBuffer, _fence);
        _fence.WaitForThenReset();

        foreach(var texture in _usedTextures)
        {
            texture.Ready();
        }

        _usedTextures.Clear();

        _device.StagingBuffer.HackBufferFree();
    }
}
