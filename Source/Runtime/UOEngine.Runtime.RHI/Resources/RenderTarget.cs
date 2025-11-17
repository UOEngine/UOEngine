namespace UOEngine.Runtime.RHI;

public class RhiRenderTarget
{
    public uint Width => Texture.Width;
    public uint Height => Texture.Height;

    public IRenderTexture Texture { get; private set; }

    public void Setup(IRenderTexture texture)
    {
        Texture = texture;
    }
}
