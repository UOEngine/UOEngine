using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UOEngine.Runtime.Renderer.Resources;

namespace UOEngine.Runtime.Renderer;

public class RenderFactory
{
    private readonly GraphicsDevice _graphicsDevice;

    public RenderFactory()
    {
    }

    public UOETexture CreateTexture(int width, int height)
    {
        var texture = new UOETexture(_graphicsDevice, width, height);

        return texture;
    }

    public ShaderInstance CreateShaderInstance(byte[] shaderBytecode)
    {
        return new ShaderInstance(_graphicsDevice, shaderBytecode);
    }

}
