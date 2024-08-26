using System.Diagnostics;

using Silk.NET.Vulkan;
using VkSemaphore = Silk.NET.Vulkan.Semaphore;

namespace UOEngine.Runtime.Rendering
{
    public enum ERenderCommandBufferState
    {
        Invalid,
        NotAllocated,
        Ready,
        Reset,
        Recording,
        Ended,
        Submitted,
        Finished
    }

    public class RenderCommandBuffer
    {
        public CommandBuffer                Handle { get; private set; }
        public bool                         IsUploadBuffer { get; private set;  }
        public bool                         IsInsideRenderPass { get; private set;}
        public ERenderCommandBufferState    State { get; private set; }

        // This buffer has finished its execution and will signal the below semaphore.
        public VkSemaphore                  ExecutionComplete { get; private set; }

        // Do not start executing this command list until this semaphore is signaled.
        public VkSemaphore?                 WaitSemaphore { get; set; }
        public PipelineStageFlags           WaitFlags { get; set; }

        private readonly Vk                 _vk;
        private Fence                       _fence;
        private RenderDevice                _device;   

        public RenderCommandBuffer(RenderDevice device, bool bIsUploadBuffer)
        {
            _vk = Vk.GetApi();
            IsUploadBuffer = bIsUploadBuffer;

            _device = device;

            _fence = device.CreateFence();
            ExecutionComplete = device.CreateSemaphore();

            IsInsideRenderPass = false;

            State = ERenderCommandBufferState.NotAllocated;

            Allocate();
        }

        public void RefreshFenceStatus()
        {
            if(State != ERenderCommandBufferState.Submitted)
            {
                return;
            }

            if(_device.IsFenceSignaled(_fence))
            {
                State = ERenderCommandBufferState.Finished;
            }
        }

        public void Begin()
        {
            CommandBufferBeginInfo beginInfo = new()
            {
                SType = StructureType.CommandBufferBeginInfo,
            };

            if (_vk.BeginCommandBuffer(Handle, ref beginInfo) != Result.Success)
            {
                throw new Exception("failed to begin command buffer!");
            }

            State = ERenderCommandBufferState.Recording;
        }

        public void End()
        {
            Debug.Assert(IsInsideRenderPass == false);
            Debug.Assert(State == ERenderCommandBufferState.Recording);

            _vk.EndCommandBuffer(Handle);

            State = ERenderCommandBufferState.Ended;
        }

        public unsafe void BeginRenderPass(RenderPass renderPass, RenderFramebuffer framebuffer)
        {
            Debug.Assert(IsInsideRenderPass == false);

            RenderPassBeginInfo renderPassInfo = new()
            {
                SType = StructureType.RenderPassBeginInfo,
                RenderPass = renderPass,
                Framebuffer = framebuffer.Handle!.Value,
                RenderArea =
                {
                    Offset = { X = 0, Y = 0 },
                    Extent = framebuffer.RenderArea.Extent,
                }
            };

            ClearValue clearColor = new()
            {
                Color = new() { Float32_0 = 0, Float32_1 = 0, Float32_2 = 0, Float32_3 = 1 },
            };

            renderPassInfo.ClearValueCount = 1;
            renderPassInfo.PClearValues = &clearColor;

            _vk.CmdBeginRenderPass(Handle, &renderPassInfo, SubpassContents.Inline);

            Viewport viewport = new()
            {
                X = 0,
                Y = 0,
                Width = framebuffer.RenderArea.Extent.Width,
                Height = framebuffer.RenderArea.Extent.Height,
                MinDepth = 0.0f,
                MaxDepth = 1.0f
            };
            
            _vk.CmdSetViewport(Handle, 0, 1, ref viewport);

            Rect2D scissor = new()
            {
                Offset = new() { X = 0, Y = 0 },
                Extent = framebuffer.RenderArea.Extent
            };

            _vk.CmdSetScissor(Handle, 0, 1, ref scissor);

            IsInsideRenderPass = true;
        }

        public void EndRenderPass()
        {
            Debug.Assert(IsInsideRenderPass);

            _vk!.CmdEndRenderPass(Handle);

            IsInsideRenderPass = false;
        }

        private void Allocate()
        {
            Handle = _device.AllocateCommandBuffer();
            State = ERenderCommandBufferState.Ready;
        }
    }
}
