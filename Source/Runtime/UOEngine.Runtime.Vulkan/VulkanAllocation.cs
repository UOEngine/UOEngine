// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.Vulkan;

internal unsafe struct VulkanAllocation
{
    public byte* Ptr;
    public ulong Size;

    public Span<byte> AsSpan() => new(Ptr, (int)Size);
}
