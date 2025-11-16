using Microsoft.Xna.Framework.Graphics;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.Platform;
using UOEngine.Runtime.RHI;

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

    public IRenderResourceFactory RenderResourceFactory => _renderResourceFactory;

    private static IRenderResourceFactory _renderResourceFactory = null!;
    private static IWindow _window = null!;

    public static void PreSetup(IRenderResourceFactory renderResourceFactory, IWindow window)
    {
        _window = window;
        _renderResourceFactory = renderResourceFactory;
    }

    public Game()
    {
        Window = new GameWindow(_window);
    }

    public void Exit()
    {
        throw new NotImplementedException();
    }

    public void Run()
    {
        throw new NotImplementedException();
    }

    public void DoInitialise()
    {

        Initialize();
    }

    protected virtual void Initialize()
    {
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
