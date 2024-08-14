using System.Diagnostics;

using Silk.NET.Core.Contexts;
using Silk.NET.Vulkan;

namespace UOEngine.Runtime.Rendering
{
    public class RenderViewport
    {
        public RenderViewport(RenderDevice renderDevice)
        {
            SwapChain = new RenderSwapChain(renderDevice);
        }
        public void Initialise(IVkSurface? surface, uint width, uint height)
        {
            if(surface == null)
            {
                Debug.Assert(false);

                return;
            }

            SwapChain.Setup(surface, width, height);
        }

        public void Render(RenderCommandListImmediate commandList)
        {
            commandList!.BeginRenderPass(MainRenderPass, SwapChain.Extent);

            Rendering?.Invoke(commandList);

            commandList.EndRenderPass();
        }

        public RenderSwapChain                  SwapChain { get; private set; }
        public uint                             MainRenderPass { get; private set; }

        public delegate void                    RenderingEventHandler(RenderCommandListImmediate renderCommandList);
        public event RenderingEventHandler?     Rendering;
    }
}
