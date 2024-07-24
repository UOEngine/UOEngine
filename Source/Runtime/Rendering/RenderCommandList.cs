using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace UOEngine.Runtime.Rendering
{
    public class RenderCommandList: IDisposable
    {
        public RenderCommandList(CommandBuffer commandBuffer, RenderDevice renderDevice)
        {
            _commandBuffer = commandBuffer;
            _renderDevice = renderDevice;

            _vk = Vk.GetApi();
        }

        public void Dispose()
        {
            _renderDevice.EndRecording();

        }

        public void CopyBuffer(Buffer srcBuffer, Buffer destBuffer, BufferCopy copyRegion)
        {
            _vk.CmdCopyBuffer(_commandBuffer, srcBuffer, destBuffer, 1, ref copyRegion);

        }
        public void CopyBufferToImage(Buffer buffer, Image image, ImageLayout imageLayout, uint regionCount, BufferImageCopy bufferImageCopy)
        {
            _vk.CmdCopyBufferToImage(_commandBuffer, buffer, image, ImageLayout.TransferDstOptimal, regionCount, ref bufferImageCopy);
        }

        public void PipelineBarrier(PipelineStageFlags sourceStage, PipelineStageFlags destinationStage, ImageMemoryBarrier imageMemoryBarrier)
        {
            unsafe
            {
                _vk.CmdPipelineBarrier(_commandBuffer, sourceStage, destinationStage, 0, 0, null, 0, null, 1, ref imageMemoryBarrier);
            }
        }

        private CommandBuffer           _commandBuffer;
        private readonly RenderDevice   _renderDevice;
        private readonly Vk             _vk;

    }
}
