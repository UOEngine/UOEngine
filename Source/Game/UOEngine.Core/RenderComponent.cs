using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Editor
{
    internal class RenderComponent
    {
        public readonly Texture2D Texture;

        public RenderComponent(uint width, uint height)
        {
            Texture = new Texture2D(width, height);
        }
    }
}
