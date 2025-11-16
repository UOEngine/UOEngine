namespace UOEngine.Runtime.RHI;

public class RhiRenderTarget
{
    public IRenderTexture Texture { get; private set; }

    public void Setup(IRenderTexture texture)
    {
        Texture = texture;
    }
}
