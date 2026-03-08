// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Vulkan;

//internal class VulkanIndexBuffer: IRhiIndexBuffer
//{
//    private readonly VulkanBuffer _buffer;

//    private ushort[] _indices = [];

//    internal VulkanIndexBuffer(VulkanDevice device, uint size, string name)
//    {
//        _buffer = new VulkanBuffer(device, new RhiBufferDescription
//        {
//            Size = size,
//            Stride = sizeof(ushort),
//            Usage = RhiBufferUsageFlags.Index,
//            Name = name
//        });
//    }

//    public void SetData(int offsetInBytes, nint data, int byteLength)
//    {
//        throw new NotImplementedException();
//    }

//    public void SetData(ReadOnlySpan<ushort> data)
//    {
//        _indices = data.ToArray();

//        throw new NotImplementedException();
//    }

//    public void Upload()
//    {
//        _buffer.Upload();
//    }
//}
