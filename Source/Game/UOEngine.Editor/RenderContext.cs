using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine
{
    public class RenderContext
    {
        public static RenderContext Instance =>     _instance.Value;
        private static readonly Lazy<RenderContext> _instance = new(() => new RenderContext());

        public RenderContext()
        {

        }

        public void Draw()
        {

        }
    }
}
