namespace UOEngine.UltimaOnline.Assets
{
    public class UOAssetLoader
    {
        public void LoadAllFiles(string ultimaOnlineDirectory)
        {
            var GumpAssetFile = new UOFile(Path.Combine(ultimaOnlineDirectory, "gumpartLegacyMUL.uop"));

            GumpAssetFile.Load();

        }
    }
}
