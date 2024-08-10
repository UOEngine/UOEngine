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

        public void BindIndexBuffer(RenderBuffer buffer)
        {
            _vk.CmdBindIndexBuffer(_commandBuffer, buffer.DeviceBuffer, 0, IndexType.Uint16);
        }

        public void BindVertexBuffer(RenderBuffer buffer)
        {
            //_vk.CmdBindVertexBuffers(_commandBuffer, 0, 0, ref buffer.DeviceBuffer, 0);
        }

        public void PipelineBarrier(PipelineStageFlags sourceStage, PipelineStageFlags destinationStage, ImageMemoryBarrier imageMemoryBarrier)
        {
            unsafe
            {
                _vk.CmdPipelineBarrier(_commandBuffer, sourceStage, destinationStage, 0, 0, null, 0, null, 1, ref imageMemoryBarrier);
            }
        }

        public void DrawIndexed(uint indexCount, uint instanceCount)
        {
            _vk.CmdDrawIndexed(_commandBuffer, indexCount, instanceCount, 0, 0, 0);
        }

        public void BindShader(Shader shader)
        {
            BindShader(shader.GetHashCode());
        }

        public void BindShader(int shaderId)
        {
            _currentShaderId = shaderId;

            PipelineStateObjectDescription pso = _renderDevice.GetPipelineStateObjectDescription(shaderId);

            _vk.CmdBindPipeline(_commandBuffer, PipelineBindPoint.Graphics, pso.PSO);
        }

        private CommandBuffer           _commandBuffer;
        private readonly RenderDevice   _renderDevice;
        private readonly Vk             _vk;

        private int                     _currentShaderId;


    }
}
