using Microsoft.Extensions.DependencyInjection;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using System.Diagnostics.CodeAnalysis;

using UOEngine.Runtime.Core;

namespace UOEngine.Runtime.Rendering
{
    public class Renderer: IPlugin
    {
        public Renderer()
        {
            //Context = new RenderDeviceContext();
            Device = new RenderDevice();
        }

        public override void Initialise(IServiceProvider services)
        {
            var gameLoop = services.GetRequiredService<GameLoop>();

            gameLoop.FrameEnded += RenderFrame;

            var shaderCompiler = new ShaderCompiler();

            shaderCompiler.Compile(Paths.Intermediate + "/Shaders");
        }

        public RenderViewport CreateViewport(IVkSurface? surface, uint width, uint height)
        {
            if (surface == null)
            {
                throw new Exception("Renderer.CreateViewport: Surface is null.");
            }

            var viewport = new RenderViewport(Device);

            viewport.Initialise(surface, width, height);

            _viewports.Add(viewport);

            return viewport;
        }

        public void RenderFrame(float deltaSeconds)
        {
            FrameStart?.Invoke();

            foreach (var viewport in _viewports)
            {
                viewport.Render();
                viewport.Present();
            }

            FrameEnd?.Invoke();
        }

        public static string[] PtrToStringArray(nint data, int count)
        {
            return SilkMarshal.PtrToStringArray(data, count); // dirty
        }

        public RenderDevice                     Device { get; private set; }
        //public RenderDeviceContext              Context { get; private set; }

        public delegate void                    FrameStartEventHandler();
        public event FrameStartEventHandler?    FrameStart;

        public delegate void                    FrameEndEventHandler();
        public event FrameEndEventHandler?      FrameEnd;

        public IReadOnlyList<RenderViewport>    Viewports => _viewports;

        private List<RenderViewport>            _viewports = [];    

    }
}
