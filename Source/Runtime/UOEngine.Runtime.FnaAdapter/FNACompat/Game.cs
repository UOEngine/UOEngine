using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework.Graphics;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.Platform;
using UOEngine.Runtime.Renderer;
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

    public GraphicsDevice GraphicsDevice
    {
        get
        {
            if (_graphicsDeviceManager == null)
            {
                _graphicsDeviceManager = Services.GetService<GraphicsDeviceManager>();

                if (_graphicsDeviceManager == null)
                {
                    throw new InvalidOperationException(
                        "No Graphics Device Service"
                    );
                }
            }
            return _graphicsDeviceManager.GraphicsDevice;
        }
    }

    public IRenderResourceFactory RenderResourceFactory => _renderResourceFactory;
    public IServiceProvider ServiceProvider => _serviceProvider;

    public readonly GameServiceContainer Services = new(); 

    private readonly GameTime _gameTime = new();

    private static IRenderResourceFactory _renderResourceFactory = null!;
    private static IServiceProvider _serviceProvider = null!;

    private GraphicsDeviceManager? _graphicsDeviceManager;

    private Stopwatch _gameTimer;
    private long _previousTicks = 0;

    public static void PreSetup(IServiceProvider serviceProvider)
    {
        // Set up with what is required from UOEngine.
        _serviceProvider = serviceProvider;
    }

    public Game()
    {
        Window = new GameWindow(GetService<IWindow>());

        _renderResourceFactory = GetService<IRenderResourceFactory>();
        var applicationLoop = GetService<ApplicationLoop>();

        applicationLoop.OnUpdate += UpdateInternal;

        var inputManager = GetService<InputManager>();

        GetService<RenderSystem>().OnFrameBegin += DrawInternal;

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

        _gameTimer = Stopwatch.StartNew();
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
    }

    protected virtual bool BeginDraw()
    {
        return true;
    }

    protected virtual void EndDraw()
    {
    }

    protected virtual void Draw(GameTime gameTime)
    {
    }

    public void Dispose()
    {
    }

    private void UpdateInternal(float deltaSeconds)
    {
        long currentTicks = _gameTimer.Elapsed.Ticks;
        TimeSpan timeAdvanced = TimeSpan.FromTicks(currentTicks - _previousTicks);
        _gameTime.ElapsedGameTime = timeAdvanced;

        _previousTicks = currentTicks;

        Update(_gameTime);
    }

    private void DrawInternal(IRenderContext renderContext)
    {
        if(BeginDraw())
        {
            Draw(_gameTime);
            EndDraw();
        }
    }

    private T GetService<T>() => _serviceProvider.GetRequiredService<T>();
}
