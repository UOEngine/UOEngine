using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Renderer;

public class TextureCache
{
    public static TextureCache Instance => _instance.Value;

    private static readonly Lazy<TextureCache> _instance = new(() => new TextureCache());
    private int[] _renderIds = new int[MaxTextures];
    private int _numTexturesAdded = 0;

    private Memory<Texture2D> _textures = new Texture2D[MaxTextures];
    private Span<Texture2D> Textures => _textures.Span;

    private static readonly int MaxTextures = 2048;

    private TextureCache()
    {
        Array.Fill(_renderIds, -1);
    }

    public bool Contains(int textureId)
    {
        int renderIndex = _renderIds[textureId];

        if(renderIndex == -1)
        {

            return false;
        }

        Debug.Assert(Textures[renderIndex] != null);

        return true;
    }

    public int GetRenderResourceId(int textureId)
    {
        Debug.Assert(Contains(textureId));

        return _renderIds[textureId];
    }

    public int AddTexture(int textureId, Texture2D texture)
    {
        int renderIndex = -1;

        if(Contains(textureId))
        {
            return _renderIds[textureId];
        }

        renderIndex = _numTexturesAdded;

        _renderIds[textureId] = renderIndex;

        Textures[renderIndex] = texture;

        _numTexturesAdded++;

        RenderContext.SetBindlessTextures(_textures.Span, (uint)_numTexturesAdded);

        return 0;
    }
}
