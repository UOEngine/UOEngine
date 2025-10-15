namespace UOEngine.Runtime.RHI;

public interface IRenderResourceFactory
{
    IRenderTexture CreateTexture(int width, int height);
}
