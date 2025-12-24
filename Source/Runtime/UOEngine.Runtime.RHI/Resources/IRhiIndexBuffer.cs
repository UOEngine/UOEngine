// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.RHI;

public interface IRhiIndexBuffer
{
    public void SetData(int offsetInBytes, IntPtr data, int byteLength);

    public void SetData(ReadOnlySpan<ushort> data);

    public void Upload();
}
