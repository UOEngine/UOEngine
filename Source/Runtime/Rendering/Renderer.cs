using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

using UOEngine.Runtime.Core;

namespace UOEngine.Runtime.Rendering
{
    public class Renderer: IPlugin
    {
        public Renderer()
        {
            Context = new RenderDeviceContext();
            Device = new RenderDevice(Context);

        }

        public override void Initialise(IServiceProvider services)
        {
            var gameLoop = services.GetRequiredService<GameLoop>();

            gameLoop.FrameEnded += OnFrameEnd;

            var shaderCompiler = new ShaderCompiler();

            shaderCompiler.Compile(Paths.Intermediate + "/Shaders");
        }

        public void OnFrameEnd(float deltaSeconds)
        {
            FrameStart?.Invoke();

            Context.Tick();

            FrameEnd?.Invoke();
        }
    
        public RenderDevice                     Device { get; private set; }
        public RenderDeviceContext              Context { get; private set; }

        public delegate void                    FrameStartEventHandler();
        public event FrameStartEventHandler?    FrameStart;

        public delegate void                    FrameEndEventHandler();
        public event FrameEndEventHandler?    FrameEnd;

    }
}
