using Microsoft.Extensions.DependencyInjection;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.Rendering;

namespace UOEngine.Apps.Client
{
    internal class ClientProgram: Program
    {
        static void Main(string[] args)
        {
            var clientProgram = new ClientProgram();

            clientProgram.Run(args);
        }

        protected override bool Initialise()
        {
           RegisterService<Input>();
           RegisterService<Window>();
           RegisterService<RenderDevice>();
           RegisterService<RenderDeviceContext>();

            return true;
        }

        protected override void OnServicesCreated(IServiceProvider serviceProvider)
        {
            bool bEnableValidationLayers = true;

            var window = serviceProvider.GetRequiredService<Window>();

            window.Init();

            var input = serviceProvider.GetRequiredService<Input>();

            input.Initialise(window.GetHandle());

            input.MouseMovedEvent += (s, e) => Console.WriteLine($"MouseMoved {e.X}, {e.Y}");

            var renderdevice = serviceProvider.GetRequiredService<RenderDevice>();

            renderdevice.Initialise(window.GetSurface(), window.Width, window.Height, bEnableValidationLayers);
        }

    }
}
