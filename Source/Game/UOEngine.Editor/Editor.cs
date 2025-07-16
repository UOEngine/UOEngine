using System.Diagnostics;
using System.Drawing;

using UOEngine.UOAssets;

namespace UOEngine
{
    public class Editor : IUOEngineApp
    {
        readonly UOAssetLoader _assetLoader = new();

        private Texture2D? _texture;

        public bool PreEngineInit()
        {
            _assetLoader.LoadAllFiles(@"D:\Program Files (x86)\Electronic Arts\Ultima Online Classic");

            return true;
        }

        public bool Initialise()
        {
            Debug.WriteLine($"Game.Initialise: Start");

            var loginBackgroundBitmap = _assetLoader.GetGump((int)EGumpTypes.LoginBackground);

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
    }
}
