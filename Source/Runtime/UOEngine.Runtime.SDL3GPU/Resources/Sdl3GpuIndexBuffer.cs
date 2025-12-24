// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Runtime.InteropServices;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.SDL3GPU;

internal class Sdl3GpuIndexBuffer: Sdl3GpuBuffer, IRhiIndexBuffer
{
    public readonly uint NumIndices;

    public Sdl3GpuIndexBuffer(Sdl3GpuDevice device, uint length, string name = "")
        : base(device, RhiBufferType.Index, length * sizeof(ushort), name)
    {
        // 16 bit only so far!
        NumIndices = length;
    }

    public void SetData(ReadOnlySpan<ushort> data) => data.CopyTo(MemoryMarshal.Cast<byte, ushort>(Data));
}
