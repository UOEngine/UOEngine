using System.Diagnostics;
using System.Drawing;

using UOEngine.UOAssets;

namespace UOEngine
{
    public class Editor : IUOEngineApp
    {
        public bool Initialise()
        {
            Debug.WriteLine($"Game.Initialise: Start");

            var assetLoader = new UOAssetLoader();

            assetLoader.LoadAllFiles(@"D:\Program Files (x86)\Electronic Arts\Ultima Online Classic");

            var loginBackgroundBitmap = assetLoader.GetGump((int)EGumpTypes.LoginBackground);

            _texture = new(loginBackgroundBitmap.Width, loginBackgroundBitmap.Height);

            _texture.SetPixels(loginBackgroundBitmap.Texels);
            _texture.Apply();

            return true;
        }

        public void Update(float tick)
        {
            RenderContext.SetShaderBindingData(_texture);
            RenderContext.Draw();
        }

        private Texture2D? _texture;
    }
}
