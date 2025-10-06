using Microsoft.Xna.Framework.Graphics;

namespace UOEngine.Runtime.Renderer;

public class UOETexture
{
    public Texture2D Resource => _texture;

    private readonly Texture2D _texture;

    public UOETexture(GraphicsDevice graphicsDevice, int width, int height)
    {
        _texture = new Texture2D(graphicsDevice, width, height);
    }

    public void SetTexels<T>(T[] texels) where T: struct
    {
        _texture.SetData(texels);
    }

    public void SetDataPointer(IntPtr pointer, int size)
    {
        _texture.SetDataPointerEXT(0, null, pointer, size);
    }
}
