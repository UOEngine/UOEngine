using System.Numerics;

namespace Microsoft.Xna.Framework.Graphics;
public class BasicEffect: Effect
{
    public BasicEffect(GraphicsDevice device)
    {

    }

    public Matrix World;
    public Matrix View;
    public Matrix Projection;

    public Texture2D Texture;
    public bool TextureEnabled;
    public bool VertexColorEnabled;

}
