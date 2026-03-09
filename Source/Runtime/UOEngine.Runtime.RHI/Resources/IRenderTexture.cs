// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.RHI;

public enum RhiRenderTextureUsage
{
    Sampler,
    ColourTarget
}

public struct RhiTextureDescription
{
    public required uint Width;
    public required uint Height;
    public required RhiRenderTextureUsage Usage;
    public string? Name;
}

public interface IRhiTextureInterop { }

public interface IRenderTexture
{
    public string Name { get; set; }

    public IntPtr Handle { get; }

    public uint Width { get; }
    public uint Height { get; }

    public Span<T> GetTexelsAs<T>() where T : unmanaged;

    public void Upload();

    public void Upload(uint x = 0, uint y = 0, uint w = 0, uint h = 0);

    void GetFeature<T>(out T? feature) where T : IRhiTextureInterop;

}
public struct RhiVkImageInterop : IRhiTextureInterop
{
    public ulong Handle;
    public uint Format;
    public uint ImageLayout;
    public uint ImageTiling;
    public uint ImageUsageFlags;
    public uint SharingMode;
}