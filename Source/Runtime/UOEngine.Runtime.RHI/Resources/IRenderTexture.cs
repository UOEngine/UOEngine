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
    public string Name;
}

public interface IRenderTexture
{
    public string Name { get; set; }

    public IntPtr Handle { get; }

    public uint Width { get; }
    public uint Height { get; }

    //public void SetData(Span<uint> texels);

    //public void SetData(Span<byte> texels);

    public Span<T> GetTexelsAs<T>() where T : unmanaged;

    public void Upload();

    public void Upload(uint x = 0, uint y = 0, uint w = 0, uint h = 0);

}
