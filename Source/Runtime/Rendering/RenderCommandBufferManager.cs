using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Runtime.Rendering
{
    internal class RenderCommandBufferManager
    {
        private RenderCommandListContext _renderCommandListContext;

        public RenderCommandBufferManager(RenderCommandListContext renderCommandListContext)
        {
            _renderCommandListContext = renderCommandListContext;
        }
    }
}
