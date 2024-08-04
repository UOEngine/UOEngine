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

            var renderDevice = serviceProvider.GetRequiredService<RenderDevice>();

            renderDevice.SwapchainDirty += (s, e) => renderDevice.SetSurfaceSize(window.Width, window.Height);

            renderDevice.Initialise(window.GetSurface(), window.Width, window.Height, bEnableValidationLayers);

            int shaderId = renderDevice.RegisterShader<SimpleShader>();

            // Fixed dir for now.
            var assetLoader = serviceProvider.GetRequiredService<UOAssetLoader>();
            
            assetLoader.LoadAllFiles("C:\\Program Files (x86)\\Electronic Arts\\Ultima Online Classic");

            var loginBackgroundBitmap = assetLoader.Gumps.GetGump(EGumpTypes.LoginBackground);

            RenderTexture2DDescription description = new RenderTexture2DDescription();

            description.Width = loginBackgroundBitmap.Width;
            description.Height = loginBackgroundBitmap.Height;
            description.Format = ERenderTextureFormat.A1R5G5B5;

            var backgroundTexture = renderDevice.CreateTexture2D(description);

            backgroundTexture.Upload(loginBackgroundBitmap.Texels);

            ushort[] quadIndices =  [0, 1, 2, 2, 3, 0];

            RenderBuffer indexBuffer = renderDevice.CreateRenderBuffer(quadIndices, ERenderBufferType.Index);

            var renderdeviceContext = serviceProvider.GetRequiredService<RenderDeviceContext>();

            renderdeviceContext.SetIndexBuffer(indexBuffer);
            renderdeviceContext.SetShader(shaderId);
            renderdeviceContext.SetTexture(0, backgroundTexture);

        }

        protected override void Shutdown(IServiceProvider serviceProvider)
        {
            serviceProvider.GetRequiredService<RenderDevice>().Shutdown();
        }

    }
}
