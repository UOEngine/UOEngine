namespace UOEngine.Runtime.RHI;

public enum RenderTextureUsage
{
    Sampler,
    ColourTarget
}

public struct RenderTextureDescription
{
    public required uint Width;
    public required uint Height;
    public required RenderTextureUsage Usage;
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

}
