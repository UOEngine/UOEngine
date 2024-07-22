using Silk.NET.Vulkan;
using UOEngine.Runtime.Core;

namespace UOEngine.Runtime.Rendering
{
    public class RenderDeviceContext: ITickable
    {
        public RenderDeviceContext()
        {
            _vk = Vk.GetApi();
        }

        public void Tick(float deltaSeconds)
        {
            RenderDevice!.OnFrameBegin();

            RenderDevice.BeginRenderPass();

            if (_bindIndexBuffer)
            {
                _vk.CmdBindIndexBuffer(RenderDevice.CurrentCommandBuffer, _indexBuffer!.DeviceBuffer, 0, IndexType.Uint16);
            }

            _vk!.CmdDrawIndexed(RenderDevice.CurrentCommandBuffer, _indexBuffer!.Length, 1, 0, 0, 0);

            RenderDevice.EndRenderPass();

            RenderDevice.Submit();
        }

        public void SetIndexBuffer(RenderBuffer indexBuffer)
        {
            if(indexBuffer != null)
            {
                _bindIndexBuffer = true;
                _indexBuffer = indexBuffer;

                return;
            }

            _bindIndexBuffer = false;
            _indexBuffer = null;
        }

        public RenderDevice?    RenderDevice;

        public RenderBuffer?    IndexBuffer { get; set; }

        private readonly Vk     _vk;
        private RenderBuffer?   _indexBuffer;
        private bool            _bindIndexBuffer = false;
    }
}
