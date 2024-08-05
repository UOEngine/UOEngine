using Silk.NET.Vulkan;

namespace UOEngine.Runtime.Rendering
{
    public class RenderCommandListImmediate: RenderCommandList
    {
        public RenderCommandListImmediate(CommandBuffer commandBuffer, RenderDevice renderDevice): base(commandBuffer, renderDevice)
        {

        }
    }
}
