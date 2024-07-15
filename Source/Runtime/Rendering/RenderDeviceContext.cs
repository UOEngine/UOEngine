using UOEngine.Runtime.Core;

namespace UOEngine.Runtime.Rendering
{
    public class RenderDeviceContext: ITickable
    {
        public RenderDeviceContext()
        {
        }

        public void Tick(float deltaSeconds)
        {
            RenderDevice!.OnFrameBegin();

            RenderDevice.BeginRenderPass();

            RenderDevice.EndRenderPass();

            RenderDevice.Submit();
        }

        public RenderDevice? RenderDevice;
    }
}
