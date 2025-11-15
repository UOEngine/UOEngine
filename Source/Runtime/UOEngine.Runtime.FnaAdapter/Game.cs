using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework;

public class Game: IDisposable
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

    public void Run()
    {

    }

    protected virtual void Initialize()
    {
        throw new NotImplementedException();
    }

    protected virtual void BeginRun()
    {
        throw new NotImplementedException();
    }

    protected virtual void UnloadContent()
    {
        throw new NotImplementedException();
    }

    protected virtual void Update(GameTime gameTime)
    {
        throw new NotImplementedException();
    }

    protected virtual bool BeginDraw()
    {
        throw new NotImplementedException();
    }

    protected virtual void EndDraw()
    {
        throw new NotImplementedException();
    }

    protected virtual void Draw(GameTime gameTime)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
    }
}
