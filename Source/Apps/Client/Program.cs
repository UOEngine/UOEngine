using Microsoft.Extensions.DependencyInjection;

using UOEngine.Runtime.Core;
using UOEngine.Runtime.Rendering;
using UOEngine.UltimaOnline.Assets;

namespace UOEngine.Apps.Client
{
    public class ClientProgram: Program
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
            RegisterService<UOAssetLoader>();
            RegisterPlugin<Renderer>();

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

            var renderer = serviceProvider.GetRequiredService<Renderer>();

            string[] extensions;

            unsafe
            {
                var glfwExtensions = window.GetSurface()!.GetRequiredExtensions(out var glfwExtensionCount);

                extensions = Renderer.PtrToStringArray((nint)glfwExtensions, (int)glfwExtensionCount);
            }
            
            renderer.Initialise(serviceProvider);

            renderer.Device.Initialise(extensions, bEnableValidationLayers);
            RenderViewport viewport = renderer.CreateViewport(window.GetSurface(), window.Width, window.Height);

            window.Resized += (size) =>
            {
                viewport.Resize((uint)size.X, (uint)size.Y);
            };

            int shaderId = renderer.Device.RegisterShader<SimpleShader>();

            // Fixed dir for now.
            var assetLoader = serviceProvider.GetRequiredService<UOAssetLoader>();
            
            assetLoader.LoadAllFiles("C:\\Program Files (x86)\\Electronic Arts\\Ultima Online Classic");

            var loginBackgroundBitmap = assetLoader.Gumps.GetGump(EGumpTypes.LoginBackground);

            RenderTexture2DDescription description = new RenderTexture2DDescription();

            description.Width = loginBackgroundBitmap.Width;
            description.Height = loginBackgroundBitmap.Height;
            description.Format = ERenderTextureFormat.A1R5G5B5;

            var backgroundTexture = renderer.Device.CreateTexture2D(description);

            backgroundTexture.Upload(loginBackgroundBitmap.Texels);

            ushort[] quadIndices =  [0, 1, 2, 2, 3, 0];

            RenderBuffer indexBuffer = renderer.Device.CreateRenderBuffer(quadIndices, ERenderBufferType.Index);

            viewport.Rendering += (commandList) =>
            {
                commandList!.BindIndexBuffer(indexBuffer);
                commandList.SetTexture(backgroundTexture, 0);
                commandList.BindShader(shaderId);
            };
        }

        protected override void Shutdown(IServiceProvider serviceProvider)
        {
            serviceProvider.GetRequiredService<RenderDevice>().Shutdown();
        }

    }
}
