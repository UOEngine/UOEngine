using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework;

public class Game
{
    public GameWindow Window
    {
        get;
        private set;
    }

    public bool IsActive;
    public GraphicsDevice GraphicsDevice;

    public void Exit()
    {
        throw new NotImplementedException();
    }

    protected virtual void Initialize()
    {

    }

    protected virtual void BeginRun()
    {

    }

    protected virtual void UnloadContent()
    {

    }

    protected virtual void Update(GameTime gameTime)
    {

    }

    protected virtual bool BeginDraw()
    {
        throw new NotImplementedException();
    }

    protected virtual void EndDraw()
    {

    }

    protected virtual void Draw(GameTime gameTime)
    {

    }


}
