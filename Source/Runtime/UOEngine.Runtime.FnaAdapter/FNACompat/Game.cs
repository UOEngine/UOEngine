using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.Xna.Framework.Graphics;

using UOEngine.Runtime.Core;
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

    public bool IsActive = true;

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

    private static IRenderResourceFactory _renderResourceFactory = null!;
    private static IServiceProvider _serviceProvider = null!;

    private GraphicsDeviceManager? _graphicsDeviceManager;

    private readonly GameTime _gameTime = new();
    private Stopwatch _gameTimer = null!;
    private TimeSpan _accumulatedElapsedTime;
    private long _previousTicks = 0;

    private bool _hasInitialized = false;

    private bool _suppressDraw;

    public static void PreSetup(IServiceProvider serviceProvider)
    {
        // Set up with what is required from UOEngine.
        _serviceProvider = serviceProvider;
    }

    public Game()
    {
        Window = new GameWindow(GetService<IWindow>());

        _renderResourceFactory = GetService<IRenderResourceFactory>();

        var inputManager = GetService<InputManager>();

        //GetService<RenderSystem>().OnFrameBegin += DrawInternal;

    }

    public void Exit()
    {
        _suppressDraw = true;
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

    public void RunOneFrame()
    {
        if (!_hasInitialized)
        {
            DoInitialize();
            _gameTimer = Stopwatch.StartNew();
            _hasInitialized = true;
        }

        Tick();
    }

    public void Tick()
    {
        AdvanceElapsedTime();

        _gameTime.ElapsedGameTime = _accumulatedElapsedTime;
        _gameTime.TotalGameTime += _gameTime.ElapsedGameTime;

        _accumulatedElapsedTime = TimeSpan.Zero;

        Update(_gameTime);

        if (BeginDraw())
        {
            Draw(_gameTime);
            EndDraw();
        }
    }

    public void Tick1()
    {
        if (!_hasInitialized)
        {
            DoInitialize();
            _gameTimer = Stopwatch.StartNew();
            _hasInitialized = true;
        }

        AdvanceElapsedTime();

        _gameTime.ElapsedGameTime = _accumulatedElapsedTime;
        _gameTime.TotalGameTime += _gameTime.ElapsedGameTime;

        _accumulatedElapsedTime = TimeSpan.Zero;

        Update(_gameTime);
    }

    public void Tick2()
    {
        if (BeginDraw())
        {
            Draw(_gameTime);
            EndDraw();
        }
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

    private T GetService<T>() where T: notnull  => _serviceProvider.GetRequiredService<T>();

    private TimeSpan AdvanceElapsedTime()
    {
        long currentTicks = _gameTimer.Elapsed.Ticks;
        TimeSpan timeAdvanced = TimeSpan.FromTicks(currentTicks - _previousTicks);
        _accumulatedElapsedTime += timeAdvanced;
        _previousTicks = currentTicks;
        return timeAdvanced;
    }

    private void DoInitialize()
    {
        Initialize();
    }
}
