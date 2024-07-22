using Microsoft.Extensions.DependencyInjection;

using UOEngine.Runtime.Core;
using UOEngine.Runtime.Rendering;
using UOEngine.UltimaOnline.Assets;

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
           RegisterService<UOAssetLoader>();

            return true;
        }

        protected override void OnServicesCreated(IServiceProvider serviceProvider)
        {
            bool bEnableValidationLayers = false;
            
            bEnableValidationLayers = true;

            var window = serviceProvider.GetRequiredService<Window>();

            window.Init();

            var input = serviceProvider.GetRequiredService<Input>();

            input.Initialise(window.GetHandle());

            var renderdevice = serviceProvider.GetRequiredService<RenderDevice>();

            renderdevice.SwapchainDirty += (s, e) => renderdevice.SetSurfaceSize(window.Width, window.Height);

            renderdevice.Initialise(window.GetSurface(), window.Width, window.Height, bEnableValidationLayers);

            // Fixed dir for now.
            serviceProvider.GetRequiredService<UOAssetLoader>().LoadAllFiles("C:\\Program Files (x86)\\Electronic Arts\\Ultima Online Classic");

            ushort[] quadIndices =  [0, 1, 2, 2, 3, 0];

            renderdevice.CreateRenderBuffer(quadIndices, ERenderBufferType.Index);

        }

        protected override void Shutdown(IServiceProvider serviceProvider)
        {
            serviceProvider.GetRequiredService<RenderDevice>().Shutdown();
        }

    }
}
