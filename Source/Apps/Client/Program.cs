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
            var window = serviceProvider.GetRequiredService<Window>();

            window.Init();

            var input = serviceProvider.GetService<Input>();

            input!.Initialise(window.GetHandle());

            input!.MouseMovedEvent += (s, e) => Console.WriteLine($"MouseMoved {e.X}, {e.Y}");
        }

    }
}
