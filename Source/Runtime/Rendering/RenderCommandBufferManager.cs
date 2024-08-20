
namespace UOEngine.Runtime.Rendering
{
    public class RenderCommandBufferManager
    {
        public RenderCommandBuffer?         ActiveCommandBuffer { get; private set; }

        public bool                         HasActiveCommandBuffer => ActiveCommandBuffer != null;
        public bool                         HasActiveUploadBuffer => _activeUploadBuffer != null;

        private List<RenderCommandBuffer>   _commandBuffers = new List<RenderCommandBuffer>(8);
        private RenderDevice                _device;
        private RenderCommandBuffer?        _activeUploadBuffer;

        public RenderCommandBufferManager(RenderDevice renderDevice)
        {
            _device = renderDevice;
        }

        public void Initialise()
        {
            PrepareNewActiveCommandBuffer();
        }

        public void SubmitActive()
        {
            ActiveCommandBuffer!.End();

            _device.Submit(ActiveCommandBuffer);

            ActiveCommandBuffer = null;
        }

        public RenderCommandBuffer GetUploadCommandBuffer()
        {
            if(_activeUploadBuffer != null)
            {
                return _activeUploadBuffer;
            }

            foreach(RenderCommandBuffer buffer in _commandBuffers)
            {
                buffer.RefreshFenceStatus();

                if(buffer.IsUploadBuffer)
                {
                    if(buffer.State == ERenderCommandBufferState.Ready || (buffer.State == ERenderCommandBufferState.Reset))
                    {
                        buffer.Begin();
                        _activeUploadBuffer = buffer;
                        return buffer;
                    }
                }
            }

            var newBuffer = new RenderCommandBuffer(_device, true);
            
            newBuffer.Begin();
            _activeUploadBuffer = newBuffer;

            return newBuffer;
        }

        public void SubmitUploadBuffer()
        {
            _activeUploadBuffer!.End();

            _device?.Submit(_activeUploadBuffer);

            _activeUploadBuffer = null;
        }

        public void PrepareNewActiveCommandBuffer()
        {
            foreach(RenderCommandBuffer buffer in _commandBuffers)
            {
                buffer.RefreshFenceStatus();

                if(buffer.IsUploadBuffer == false)
                {
                    if(buffer.State == ERenderCommandBufferState.Finished)
                    {
                        buffer.Begin();
                        ActiveCommandBuffer = buffer;

                        return;
                    }
                }
            }

            var newBuffer = new RenderCommandBuffer(_device, false);
            _commandBuffers.Add(newBuffer);

            newBuffer.Begin();
            ActiveCommandBuffer = newBuffer;
        }
    }
}
