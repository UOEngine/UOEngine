using System.Diagnostics;

using Silk.NET.Vulkan;

namespace UOEngine.Runtime.Rendering
{
    public class RenderFramebuffer
    {
        public Framebuffer?     Handle { get; private set; }
        public Rect2D           RenderArea { get; private set; }

        private RenderDevice    _device;

        public unsafe RenderFramebuffer(RenderDevice renderDevice, in RenderTargetInfo renderTargetInfo, RenderPass renderPass)
        {
            _device = renderDevice;

            ImageView imageView = renderTargetInfo.Texture.ShaderResourceView!.Value;

            FramebufferCreateInfo createInfo = new()
            {
                SType = StructureType.FramebufferCreateInfo,
                RenderPass = renderPass,
                AttachmentCount = 1,
                PAttachments = &imageView,
                Width = renderTargetInfo.Texture.Description.Width,
                Height = renderTargetInfo.Texture.Description.Height,
                Layers = 1,
            };

            Console.WriteLine("Creating frame buffer");

            Vulkan.VkCreateFrameBuffer(_device.Handle, createInfo, out var framebuffer);

            Handle = framebuffer;

            RenderArea = new()
            {
                Offset = { X = 0, Y = 0 },
                Extent = { Width = renderTargetInfo.Texture.Description.Width, Height = renderTargetInfo.Texture.Description.Height }
            };
        }
        ~RenderFramebuffer()
        {
            Debug.Assert(Handle == null);
        }

        public void Destroy()
        {
            _device.DestroyFramebuffer(Handle!.Value);
            Handle = null;
        }

    }
}
