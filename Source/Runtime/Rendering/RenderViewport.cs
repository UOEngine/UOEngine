using System.Diagnostics;

using Silk.NET.Core.Contexts;
using Silk.NET.SDL;
using Silk.NET.Vulkan;
using VkSemaphore = Silk.NET.Vulkan.Semaphore;


namespace UOEngine.Runtime.Rendering
{
    public class RenderViewport
    {
        public delegate void                RenderingEventHandler(RenderCommandListContextImmediate renderCommandList);
        public event RenderingEventHandler? Rendering;

        public RenderSwapChain?             SwapChain { get; private set; }
        public RenderPass                   MainRenderPass { get; private set; }

        private RenderDevice                _renderDevice;
        private Framebuffer[]?              _framebuffers;

        private uint                        _width;
        private uint                        _height;

        private IVkSurface?                 _surface;

        private ERenderTextureFormat        _pixelFormat = ERenderTextureFormat.B8G8R8A8;

        public RenderViewport(RenderDevice renderDevice)
        {
            _renderDevice = renderDevice;
        }
        public void Initialise(IVkSurface? surface, uint width, uint height)
        {
            if(surface == null)
            {
                Debug.Assert(false);

                return;
            }

            _width = width;
            _height = height;
            _surface = surface;

            MainRenderPass = _renderDevice.CreateRenderPass(_pixelFormat);

            CreateSwapChain();
        }

        public void Render()
        {
            if(SwapChain == null)
            {
                return;
            }

            int imageIndex = SwapChain!.AcquireImageIndex(out var imagePresentedSemaphore);

            if(imageIndex < 0)
            {
                return;
            }

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
            if (SwapChain == null)
            {
                return;
            }

            VkSemaphore executionComplete = _renderDevice.ImmediateContext!.ActiveCommandBuffer.ExecutionComplete;

            _renderDevice.ImmediateContext!.Submit();

            SwapChain!.Present(executionComplete);

            // Wait to be presented..for now.
            _renderDevice.WaitUntilIdle();

            _renderDevice.ImmediateContext.CommandBufferManager.PrepareNewActiveCommandBuffer();

        }

        public void Resize(uint width, uint height)
        {
            _width = width;
            _height = height;

            if(SwapChain != null)
            {
                DestroySwapChain();
            }

            if (width == 0 || height == 0)
            {
                return;
            }

            CreateSwapChain();
        }

        private void CreateSwapChain()
        {
            SwapChain = new RenderSwapChain(_renderDevice);

            SwapChain.Setup(_surface!, _width, _height, _pixelFormat);

            _framebuffers = new Framebuffer[SwapChain.ShaderResourceViews.Length];

            for (int i = 0; i < _framebuffers.Length; i++)
            {
                _framebuffers[i] = _renderDevice.CreateFramebuffer(SwapChain.ShaderResourceViews[i], _width, _height, MainRenderPass);
            }

        }

        private void DestroySwapChain()
        {
            _renderDevice.SubmitAndFlush();
            _renderDevice.WaitUntilIdle();

            SwapChain!.Destroy();

            foreach(Framebuffer framebuffer in _framebuffers!)
            {
                _renderDevice.DestroyFramebuffer(framebuffer);
            }

            _framebuffers = [];

            _framebuffers = null;

            SwapChain = null;
        }
    }
}
