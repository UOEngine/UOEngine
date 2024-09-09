using UOEngine.UltimaOnline.Assets.Maps;

namespace UOEngine.UltimaOnline.Assets
{
    public class UOAssetLoader
    {
        public UOFile?                  Gumps { get; private set; }

        public readonly UOMapAssets     MapAssets = new();

        public UOAssetLoader() 
        {
        }

        public void LoadAllFiles(string ultimaOnlineDirectory)
        {
            LoadGumps(ultimaOnlineDirectory);

            MapAssets.Load(ultimaOnlineDirectory);
        }

        private void LoadGumps(string ultimaOnlineDirectory)
        {
            var gumpAssets = Path.Combine(ultimaOnlineDirectory, "gumpartLegacyMUL.uop");

            Gumps = new UOFile(gumpAssets);

            Gumps.Load("build/gumpartlegacymul/{0:D8}.tga", true);
            // gump.def?
        }


    }
}
