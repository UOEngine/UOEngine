namespace UOEngine.UltimaOnline.Assets
{
    public class UOAssetLoader
    {
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
            // gump.def?
        }
    }
}
