
namespace UOEngine.Runtime.Rendering
{
    public readonly struct RenderTargetInfo
    {
        public readonly RenderTexture2D Texture;

        public RenderTargetInfo(RenderTexture2D texture)
        {
            Texture = texture;
        }
    }
}
