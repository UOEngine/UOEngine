
namespace UOEngine.UltimaOnline.Assets
{
    public class UOAssetLoader
    {
        public UOAssetLoader() 
        {
            Gumps = new UOFile();
        }

        public void LoadAllFiles(string ultimaOnlineDirectory)
        {

            LoadGumps(ultimaOnlineDirectory);
        }

        private void LoadGumps(string ultimaOnlineDirectory)
        {
            var gumpAssets = Path.Combine(ultimaOnlineDirectory, "gumpartLegacyMUL.uop");

            Gumps.Load(gumpAssets, true);
            // gump.def?
        }

        public UOFile Gumps { get; private set; }
    }
}
