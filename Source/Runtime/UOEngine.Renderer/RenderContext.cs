using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UOEngine.Runtime.Renderer.Resources;

namespace UOEngine.Runtime.Renderer;

public class RenderContext
{
    public Matrix View { get; set; }

    public ShaderInstance ShaderInstance
    {
        get { return _shaderInstance; }
        set 
        {
            if(value == _shaderInstance)
            {
                return;
            }

            _shaderInstance = value;
            _dirty = true;
        }
    }

    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;
    private ShaderInstance _shaderInstance;

    private UOETexture _texture;
    private bool _dirty = true;

    public RenderContext(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = new SpriteBatch(graphicsDevice);
    }

    public void Clear()
    {
        _graphicsDevice.Clear(Color.CornflowerBlue);
    }

    public void SetTexture(UOETexture texture)
    {
        _texture = texture;
    }

    public void Draw()
    {
        if(_dirty)
        {
            _shaderInstance.Bind();

            _dirty = false;
        }
        //_spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, View);
        //_spriteBatch.Draw(_texture.Resource, new Vector2(0, 0), Color.White);
        //_spriteBatch.End();
        //_graphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 6, 0, 2);
    }
}
