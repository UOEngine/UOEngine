using UOEngine.Runtime.Rendering;

namespace UOEngine.UltimaOnline.Assets
{
    public class UOAssetLoader
    {
        public UOAssetLoader(RenderDevice renderDevice) 
        { 
            _renderDevice = renderDevice;
        }
        public void LoadAllFiles(string ultimaOnlineDirectory)
        {

            LoadGumps(ultimaOnlineDirectory);
        }

        private void LoadGumps(string ultimaOnlineDirectory)
        {
            var gumpAssets = Path.Combine(ultimaOnlineDirectory, "gumpartLegacyMUL.uop");

            var gumpFile = new UOFile();

            gumpFile.Load(gumpAssets, true);

            var loginBackgroundBitmap = gumpFile.GetGump(EGumpTypes.LoginBackground);

            RenderTexture2DDescription description = new RenderTexture2DDescription();

            description.Width = loginBackgroundBitmap.Width;
            description.Height = loginBackgroundBitmap.Height;
            description.Format = ERenderTextureFormat.R5G5B5A1;

            var texels = new byte[description.Width * description.Height * 2];

            Buffer.BlockCopy(loginBackgroundBitmap.Texels, 0, texels, 0, texels.Length);

            _renderDevice.CreateTexture2D(description, texels);
            // gump.def?
        }

        private RenderDevice _renderDevice;
    }
}
