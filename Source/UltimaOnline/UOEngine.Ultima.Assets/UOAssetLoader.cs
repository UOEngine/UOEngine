
namespace UOEngine.UltimaOnline.Assets
{
    public class UOAssetLoader
    {
        public readonly UOFile      Gumps;

        public const int            NumMaps = 6;
        public readonly UOFile[]    MapLegacyMUL = new UOFile[NumMaps];

        public UOAssetLoader() 
        {
            Gumps = new UOFile();
        }

        public void LoadAllFiles(string ultimaOnlineDirectory)
        {
            LoadMaps(ultimaOnlineDirectory);
            LoadGumps(ultimaOnlineDirectory);
        }

        private void LoadGumps(string ultimaOnlineDirectory)
        {
            var gumpAssets = Path.Combine(ultimaOnlineDirectory, "gumpartLegacyMUL.uop");

            Gumps.Load(gumpAssets, true);
            // gump.def?
        }

        private void LoadMaps(string  ultimaOnlineDirectory)
        {
            for (int i = 0; i < NumMaps; i++)
            {
                string path = Path.Combine(ultimaOnlineDirectory, $"map{i}LegacyMUL.uop");

                MapLegacyMUL[i] = new UOFile();

                MapLegacyMUL[i].Load(path, false);
            }
        }


    }
}
