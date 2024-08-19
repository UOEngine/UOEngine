using System.Diagnostics;

using Silk.NET.Core.Contexts;
using Silk.NET.Vulkan;
using VkSemaphore = Silk.NET.Vulkan.Semaphore;


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

            RenderCommandListContextImmediate context = _renderDevice.ImmediateContext!;

            RenderCommandBuffer commandBuffer = context!.CommandBufferManager.ActiveCommandBuffer!;
            
            commandBuffer.WaitSemaphore = imagePresentedSemaphore;
            commandBuffer.WaitFlags = PipelineStageFlags.ColorAttachmentOutputBit;

            context!.BeginRenderPass(MainRenderPass, _framebuffers![imageIndex], SwapChain.Extent);

            Rendering?.Invoke(context);

            context.EndRenderPass();
        }

        public void Present()
        {
            VkSemaphore executionComplete = _renderDevice.ImmediateContext!.ActiveCommandBuffer.ExecutionComplete;

            _renderDevice.ImmediateContext!.Submit();

            SwapChain.Present(executionComplete);

            // Wait to be presented..for now.
            _renderDevice.WaitUntilIdle();

            _renderDevice.ImmediateContext.CommandBufferManager.PrepareNewActiveCommandBuffer();

        }

        public RenderSwapChain                  SwapChain { get; private set; }
        public RenderPass                       MainRenderPass { get; private set; }

        public delegate void                    RenderingEventHandler(RenderCommandListContextImmediate renderCommandList);
        public event RenderingEventHandler?     Rendering;

        private RenderDevice                    _renderDevice;
        private Framebuffer[]?                  _framebuffers;
    }
}
