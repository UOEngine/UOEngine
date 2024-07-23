using Silk.NET.Vulkan;

namespace UOEngine.Runtime.Rendering
{
    public class RenderCommandList: IDisposable
    {
        public RenderCommandList(CommandBuffer commandBuffer, RenderDevice renderDevice)
        {
            Buffer = commandBuffer;
            _renderDevice = renderDevice;
        }

        public void Dispose()
        {
            _renderDevice.EndRecording();

        }

        public CommandBuffer Buffer { get; private set; }

        private readonly RenderDevice   _renderDevice;

    }
}
