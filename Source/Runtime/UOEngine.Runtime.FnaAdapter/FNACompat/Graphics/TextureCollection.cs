namespace Microsoft.Xna.Framework.Graphics;

public class TextureCollection
{
    public Texture? this[int index]
    {
        get
        {
            return _textures[index];
        }
        set
        {
            _textures[index] = value;
            _modifiedSamplers[index] = true;
        }
    }

    private readonly Texture?[] _textures;
    private readonly bool[] _modifiedSamplers;

    internal TextureCollection(int slots, bool[] modSamplers)
    {
        _textures = new Texture[slots];
        _modifiedSamplers = modSamplers;

        for (int i = 0; i < _textures.Length; i += 1)
        {
            _textures[i] = null;
        }
    }
}
