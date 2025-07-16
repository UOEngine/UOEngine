using UOEngine.PackageFile;

namespace UOEngine.UOAssets
{
    public class UOAssetLoader
    {
        public readonly UOMapAssets     MapAssets = new();
        public UOPackageFile            Gumps { get; private set; }

        public void LoadAllFiles(string ultimaOnlineDirectory)
        {
            LoadGumps(ultimaOnlineDirectory);

            //MapAssets.Load(ultimaOnlineDirectory);
        }

        public UOBitmap GetGump(int idx)
        {
            var bitmap = new UOBitmap();

            bitmap.DeserialiseFromUOPackageFileResource(Gumps.GetResource(idx));

            return bitmap;
        }

        private void LoadGumps(string ultimaOnlineDirectory)
        {
            var gumpAssets = Path.Combine(ultimaOnlineDirectory, "gumpartLegacyMUL.uop");

            Gumps = new UOPackageFile(gumpAssets);

            Gumps.Load("build/gumpartlegacymul/{0:D8}.tga", true);
            // gump.def?
        }
    }
}
