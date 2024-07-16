using Silk.NET.Vulkan;

namespace UOEngine.Runtime.Rendering
{
    public enum ERenderSubresourceState
    {
        None = 0,
        Read,
        RenderTarget,
        ShaderResource,
    }

    public class Texture2D
    {
        public Texture2D(uint width, uint height) 
        {
            this.width = width;
            this.height = height;
        }

        public void SubresourceTransition(ERenderSubresourceState newState)
        {

        }

        //private ImageView? imageView;
        //private Image? image;

        private uint width;
        private uint height;
    }
}
