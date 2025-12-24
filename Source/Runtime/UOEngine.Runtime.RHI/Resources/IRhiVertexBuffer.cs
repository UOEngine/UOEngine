// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.RHI;

public interface IRhiVertexBuffer
{
    uint Size { get; }
    uint Stride { get; }

    public void SetData(int offsetInBytes, IntPtr data, int dataLength);

    public void Upload();

    public void CleanUp();
}
