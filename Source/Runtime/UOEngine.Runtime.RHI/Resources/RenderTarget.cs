namespace UOEngine.Runtime.RHI.Resources;

public class RenderTarget
{
    public IRenderTexture Texture { get; private set; }

    public void Setup(IRenderTexture texture)
    {
        Texture = texture;
    }
}
