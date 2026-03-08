// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.

namespace UOEngine.Runtime.Vulkan;

internal class VulkanDeferredDeletionQueue
{
    public uint CurrentFrame;

    struct Entry
    {
        public uint FrameNumber;
        public VulkanMemoryAllocation Allocation;
    }

    private List<Entry> _entries = [];

    private readonly VulkanDevice _device;

    internal VulkanDeferredDeletionQueue(VulkanDevice device)
    {
        _device = device;
    }

    internal void EnqueueResourceAllocation(in VulkanMemoryAllocation allocation)
    {
        _entries.Add(new Entry
        {
            FrameNumber = CurrentFrame,
            Allocation = allocation
        });
    }

    internal void ReleaseResources(uint currentFrame)
    {
        int num = _entries.Count;
        
        for (int i = _entries.Count - 1; i >= 0; i--)
        {
            if(currentFrame >= (_entries[i].FrameNumber + 2))
            {
                _device.MemoryManager.Free(_entries[i].Allocation, false);

                _entries.RemoveAt(i);
            }
        }
    }
}
