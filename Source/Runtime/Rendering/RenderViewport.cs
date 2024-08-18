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

            _renderDevice = renderDevice;
        }
        public void Initialise(IVkSurface? surface, uint width, uint height)
        {
            if(surface == null)
            {
                Debug.Assert(false);

                return;
            }

            SwapChain.Setup(surface, width, height, ERenderTextureFormat.B8G8R8A8);

            MainRenderPass = _renderDevice.CreateRenderPass(SwapChain.TexFormat);

            _framebuffers = new Framebuffer[SwapChain.ShaderResourceViews.Length];

            for(int i = 0; i < _framebuffers.Length; i++)
            {
                _framebuffers[i] = _renderDevice.CreateFramebuffer(SwapChain.ShaderResourceViews[i], width, height, MainRenderPass);
            }
        }

        public void Render()
        {
            uint imageIndex = SwapChain.AcquireImageIndex(out var imagePresentedSemaphore);

            var commandList = _renderDevice!.ImmediateCommandList;

            commandList!.WaitSemaphore = imagePresentedSemaphore;
            commandList.WaitFlags = PipelineStageFlags.ColorAttachmentOutputBit;

            commandList!.Begin();

            commandList!.BeginRenderPass(MainRenderPass, _framebuffers![imageIndex], SwapChain.Extent);

            Rendering?.Invoke(commandList);

            commandList.EndRenderPass();
        }

        public void Present()
        {
            _renderDevice.ImmediateCommandList!.Submit();

            SwapChain.Present(_renderDevice.ImmediateCommandList.RenderingCompletedSemaphore);

            // Wait to be presented..for now.
            _renderDevice.WaitUntilIdle();

        }

        public RenderSwapChain                  SwapChain { get; private set; }
        public RenderPass                       MainRenderPass { get; private set; }

        public delegate void                    RenderingEventHandler(RenderCommandListContextImmediate renderCommandList);
        public event RenderingEventHandler?     Rendering;

        private RenderDevice                    _renderDevice;
        private Framebuffer[]?                  _framebuffers;
    }
}
