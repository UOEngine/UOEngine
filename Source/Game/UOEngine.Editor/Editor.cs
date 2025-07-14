using System.Diagnostics;
using System.Drawing;

namespace UOEngine
{
    public class Editor : IUOEngineApp
    {
        public bool Initialise()
        {
            Debug.WriteLine($"Game.Initialise: Start");

            _texture = new(128, 64);

            for (uint y = 0; y < _texture.Height; y++)
            {
                for (uint x = 0; x < _texture.Width; x++)
                {
                    _texture.SetPixel(x, y, Colour.Red);
                }
            }

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
