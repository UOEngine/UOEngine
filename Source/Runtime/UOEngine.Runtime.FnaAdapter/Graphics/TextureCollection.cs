namespace Microsoft.Xna.Framework.Graphics;

public class TextureCollection
{
    public Texture this[int index]
    {
        get
        {
            return textures[index];
        }
        set
        {
            throw new NotImplementedException();
        }
    }

    private readonly Texture[] textures;

}
